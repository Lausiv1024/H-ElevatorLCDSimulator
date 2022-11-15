using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanAce_7
{
    internal class ElevatorAnimationInfo
    {
        public readonly string[] FloorTexts;
        public readonly ElevatorDirection[] DrivingDirections;

        public ElevatorAnimationInfo(string[] floorTexts, ElevatorDirection[] drivingDirections)
        {
            FloorTexts = floorTexts;
            DrivingDirections = drivingDirections;
        }
    }
}
