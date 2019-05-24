using LibraryData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData
{
    public interface ICheckout
    {
        void Add(Checkout newCheckout);
        void CheckOutItem(int assetId, int LibraryCardId);
        void CheckInItem(int assetId);
        void PlaceHold(int assetId, int libraryCardId);
        void MarkLost(int assetId);
        void MarkFound(int assetId);


        IEnumerable<Checkout> GetAll();
        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
        IEnumerable<Hold> GetCurrentHolds(int id);


        Checkout GetById(int checkoutId);
        Checkout GetLatestCheckout(int assetId);
        bool IsCheckedOut(int assetId);


        string GetCurrentHoldPatronName(int assetId);
        string GetCurrentCheckoutPatron(int assetId);
        DateTime GetCurrentHoldPlaced(int assetId);

        

    }
}
