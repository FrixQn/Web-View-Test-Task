using UnityEngine;

namespace TestProject
{
    public readonly struct ScreenConfiguration
    {
        public readonly ScreenOrientation Orientation;
        public readonly bool AutoRotateToPortrait;
        public readonly bool AutoRotateToPortraitUpsideDown;
        public readonly bool AutoRotateToLandscapeLeft;
        public readonly bool AutoRotateToLandscapeRight;
        public ScreenConfiguration(ScreenOrientation orientation, bool autoRotateToPortait, bool autoRotateToPortaitUpsideDown, 
            bool autoRotateToLandscapeLeft, bool autoRotateToLandscapeRight) 
        { 
            Orientation = orientation;
            AutoRotateToPortrait = autoRotateToPortait;
            AutoRotateToPortraitUpsideDown = autoRotateToPortaitUpsideDown;
            AutoRotateToLandscapeLeft = autoRotateToLandscapeLeft;
            AutoRotateToLandscapeRight = autoRotateToLandscapeRight;
        }
    }
}
