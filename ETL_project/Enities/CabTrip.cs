using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETL_project.Enities
{
    public class CabTrip
    {
        [Key]
        public int Id { get; set; }

        public DateTime PickupDateTime { get; set; }

        public DateTime DropoffDateTime { get; set; }

        public int PassengerCount { get; set; }

        public float TripDistance { get; set; }

        public string StoreAndFwdFlag { get; set; }

        public int PULocationID { get; set; }

        public int DOLocationID { get; set; }

        public decimal FareAmount { get; set; }

        public decimal TipAmount { get; set; }
    }
}
