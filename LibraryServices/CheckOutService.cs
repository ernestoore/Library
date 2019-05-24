using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    public class CheckOutService : ICheckout
    {

        private LibraryContext _context;

        public CheckOutService(LibraryContext context)
        {
            _context = context;
        }


        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public Checkout GetById(int checkoutId)
        {
            return GetAll().FirstOrDefault(checkout => checkout.Id == checkoutId);
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAsset.Id == id);
        }

       
        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Where(h => h.LibraryAsset.Id == id);
        }

        public Checkout GetLatestCheckout(int assetId)
        {
            return _context.Checkouts
                .Where(c => c.LibraryAsset.Id == assetId)
                .OrderByDescending(c => c.Since)
                .FirstOrDefault();
        }

        public void MarkFound(int assetId)
        {
            var now = DateTime.Now;

                UpdateAssetStatus(assetId, "Available");
            RemoveExistingCheckouts(assetId);
            CloseExistingCheckoutHistory(assetId, now);
            
        }

        private void UpdateAssetStatus(int assetId, string newStatus)
        {
            var item = _context.LibraryAssets
                .Include(a => a.Status)
              .FirstOrDefault(c => c.Id == assetId);

            _context.Update(item);
            item.Status = _context.Statuses
                .FirstOrDefault(status => status.Name == newStatus);
        }

        private void CloseExistingCheckoutHistory(int assetId, DateTime now)
        {
            // Close any existing checkout history
            var history = _context.CheckoutHistories.FirstOrDefault(co => co.LibraryAsset.Id == assetId && co.CheckedIn == null);
            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = now;
            }

            _context.SaveChanges();
        }

        private void RemoveExistingCheckouts(int assetId)
        {
            // remove any existing checkouts on the item

            var checkout = _context.Checkouts.FirstOrDefault(co => co.LibraryAsset.Id == assetId);

            if (checkout != null)
            {
                _context.Remove(checkout);
            }
        }

        public void MarkLost(int assetId)
        {
            UpdateAssetStatus(assetId, "Lost");

            _context.SaveChanges();
        }

        
        public void CheckInItem(int assetId)
        {
            var now = DateTime.Now;

            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            // Remove any existing checkouts on the item
            RemoveExistingCheckouts(assetId);
            // Close any existing checkout history
            CloseExistingCheckoutHistory(assetId, now);
            // Look for existing Holds on the item 
            var currentHold = _context.Holds
                .Include(c => c.LibraryAsset)
                .Include(c => c.LibraryCard)
                .Where(c => c.LibraryAsset.Id == assetId);
            // If there are holds, checkout the item to the
            // LibraryCard with the earliest hold
            if (currentHold.Any())
            {
                CheckoutToEarliestHold(assetId, currentHold);
                return;
            }

            // Otherwise, update the item status to "Available"
            UpdateAssetStatus(assetId, "Available");

            _context.SaveChanges();
        }

        private void CheckoutToEarliestHold(int assetId, IQueryable<Hold> currentHold)
        {
            var earliestHold = currentHold
                .OrderBy(c => c.HoldPlaced).FirstOrDefault();

            var card = earliestHold.LibraryCard;
            _context.Remove(earliestHold);
            _context.SaveChanges();
            CheckOutItem(assetId, card.Id);
        }

        public void CheckOutItem(int assetId, int LibraryCardId)
        {
            if (IsCheckedOut(assetId))
            {
                return;
                // Add some logic to habdle feedback to the user
            }
            var item = _context.LibraryAssets.FirstOrDefault(a => a.Id == assetId);

            UpdateAssetStatus(assetId, "Checked Out");

            var libraryCard = _context.LibraryCards.Include(card => card.Checkouts)
                .FirstOrDefault(card => card.Id == LibraryCardId);

            var now = DateTime.Now;

            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckOutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard,
            };

            _context.Add(checkoutHistory);

            _context.SaveChanges();
        }

        private DateTime GetDefaultCheckOutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        public bool IsCheckedOut(int assetId)
        {
            return _context.Checkouts.Where(x => x.LibraryAsset.Id == assetId).Any();
        }
        public void PlaceHold(int assetId, int LibraryCardId)
        {
            var now = DateTime.Now;

            var asset = _context.LibraryAssets.
                Include(a => a.Status)
                .FirstOrDefault(a => a.Id == assetId);
            var libraryCard = _context.LibraryCards.FirstOrDefault(card => card.Id == LibraryCardId);

            if (asset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }

            var hold = new Hold
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = libraryCard
            };
            _context.Add(hold);
            _context.SaveChanges();

        }
        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                    .Include(h => h.LibraryCard)
                    .Include(h => h.LibraryAsset)
                    .FirstOrDefault(h => h.Id == holdId);

            var cardId = hold?.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(h => h.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName;
        }

        public DateTime GetCurrentHoldPlaced(int holdId)
        {
            return _context.Holds
                    .Include(h => h.LibraryCard)
                    .Include(h => h.LibraryAsset)
                    .FirstOrDefault(h => h.Id == holdId)
                    .HoldPlaced;
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = getCheckoutByAssetId(assetId);
            if ( checkout == null)
            {
                return "";
            };

            var cardId = checkout.LibraryCard.Id;
            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;

        }

        private Checkout getCheckoutByAssetId(int assetId)
        {
           return _context.Checkouts
                .Include(co => co.LibraryAsset)
                .Include(co => co.LibraryCard)
                .FirstOrDefault(co => co.LibraryAsset.Id == assetId);
        }
    }
}
