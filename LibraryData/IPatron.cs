using LibraryData.Models;
using System.Collections.Generic;

namespace LibraryData
{
    public interface IPatron
    {
        // Return a particular Patron by its primary key
        Patron Get(int Id);
        // Return all the collection of Patrons
        IEnumerable<Patron> GetAll();
        // Add a Patron to the database
        void Add(Patron newPatron);

        // Every Patron has a checkout history associated with it.
        IEnumerable<CheckoutHistory> GetCheckoutHistory(int patronId);
        // Every Patron has a hold history associated with it.
        IEnumerable<Hold> GetHolds(int patronId);
        // Every Patron has check out items associated with it.
        IEnumerable<Checkout> GetCheckouts(int patronId);



    }
}
