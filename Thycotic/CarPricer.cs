#region Instructions

/*
 * You are tasked with writing an algorithm that determines the value of a used car, 
 * given several factors.
 * 
 *    AGE:    Given the number of months of how old the car is, reduce its value one-half 
 *            (0.5) percent.
 *            After 10 years, it's value cannot be reduced further by age. This is not 
 *            cumulative.
 *            
 *    MILES:    For every 1,000 miles on the car, reduce its value by one-fifth of a
 *              percent (0.2). Do not consider remaining miles. After 150,000 miles, it's 
 *              value cannot be reduced further by miles.
 *            
 *    PREVIOUS OWNER:    If the car has had more than 2 previous owners, reduce its value 
 *                       by twenty-five (25) percent. If the car has had no previous  
 *                       owners, add ten (10) percent of the FINAL car value at the end.
 *                    
 *    COLLISION:        For every reported collision the car has been in, remove two (2) 
 *                      percent of it's value up to five (5) collisions.
 *                    
 * 
 *    Each factor should be off of the result of the previous value in the order of
 *        1. AGE
 *        2. MILES
 *        3. PREVIOUS OWNER
 *        4. COLLISION
 *        
 *    E.g., Start with the current value of the car, then adjust for age, take that  
 *    result then adjust for miles, then collision, and finally previous owner. 
 *    Note that if previous owner, had a positive effect, then it should be applied 
 *    AFTER step 4. If a negative effect, then BEFORE step 4.
 */

#endregion

using System;
using NUnit.Framework;

namespace CarPricer
{
    public class Car
    {
        #region Properties

        public int AgeInMonths { get; set; }
        public int NumberOfCollisions { get; set; }
        public int NumberOfMiles { get; set; }
        public int NumberOfPreviousOwners { get; set; }
        public decimal PurchaseValue { get; set; }

        #endregion
    }

    public class PriceDeterminator
    {
        #region Static Fields and Constants

        private const int MaxAgeInMonths = 12 * 10;
        private const int MaxKiloMiles = 150;
        private const int MaxCollisions = 5;
        private const int KiloMiles = 1000;       
        private const double AgeDeduction = 1 - .005;    
        private const double MilesDeduction = 1 - .002;
        private const double CollisionDeduction = 1 - .02; 
        private const decimal OwnerDeduction = 1 - .25M;
        private const decimal OwnerAdduction = 1 + .10M;

        #endregion

        #region Public Methods

        public decimal DetermineCarPrice(Car car)
        {
            AgeFactor(car);
            MilesFactor(car);
            var lowOwners = OwnersFactor(car);
            CollisionFactor(car);

            if (lowOwners)
            {
                car.PurchaseValue *= OwnerAdduction;
            }

            return decimal.Truncate(car.PurchaseValue * 100) / 100;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Step 1
        /// </summary>
        /// <param name="car">The car to evaluate.</param>
        private static void AgeFactor(Car car)
        { 
            car.PurchaseValue *= (decimal)Math.Pow(AgeDeduction, Math.Min(car.AgeInMonths, MaxAgeInMonths));
        }

        /// <summary>
        ///     Step 2
        /// </summary>
        /// <param name="car">The car to evaluate.</param>
        private static void MilesFactor(Car car)
        {
            car.PurchaseValue *= (decimal)Math.Pow(MilesDeduction, Math.Min(car.NumberOfMiles/KiloMiles, MaxKiloMiles));
        }

        /// <summary>
        ///     Step 3
        /// </summary>
        /// <param name="car">The car to evaluate.</param>
        private static bool OwnersFactor(Car car)
        {
            var val = false;
            switch (car.NumberOfPreviousOwners)
            {
                case 0:
                    val = true;
                    break;
                case 1:
                case 2:
                    break;
                default:
                    car.PurchaseValue *= OwnerDeduction;
                    break;
            }

            return val;
        }

        /// <summary>
        ///     Step 4
        /// </summary>
        /// <param name="car">The car to evaluate.</param>
        private static void CollisionFactor(Car car)
        { 
            car.PurchaseValue *= (decimal)Math.Pow(CollisionDeduction, Math.Min(car.NumberOfCollisions, MaxCollisions));
        }

        #endregion
    }

    [TestFixture]
    public class UnitTests
    {
        #region Public Methods

        [Test]
        public void CalculateCarValue()
        {
            AssertCarValue(25313.40m, 35000m, 3 * 12, 50000, 1, 1);
            AssertCarValue(19688.20m, 35000m, 3 * 12, 150000, 1, 1);
            AssertCarValue(19688.20m, 35000m, 3 * 12, 250000, 1, 1);
            AssertCarValue(20090.00m, 35000m, 3 * 12, 250000, 1, 0);
            AssertCarValue(21657.02m, 35000m, 3 * 12, 250000, 0, 1);
        }

        #endregion

        #region Private Methods

        private static void AssertCarValue(decimal expectValue, decimal purchaseValue, int ageInMonths, int numberOfMiles, int numberOfPreviousOwners, int numberOfCollisions)
        {
            var car = new Car
                      {
                          AgeInMonths = ageInMonths,
                          NumberOfCollisions = numberOfCollisions,
                          NumberOfMiles = numberOfMiles,
                          NumberOfPreviousOwners = numberOfPreviousOwners,
                          PurchaseValue = purchaseValue
                      };

            var priceDeterminator = new PriceDeterminator();
            var carPrice = priceDeterminator.DetermineCarPrice(car);
            Assert.AreEqual(expectValue, carPrice);
        }

        #endregion
    }
}