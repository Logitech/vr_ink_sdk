namespace Logitech.XRToolkit.Inking
{
    using Utils;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Drawing Modification")]
    public class DrawingModificationProperties : ScriptableObject
    {
        public float RaycastLength = 0.015f;

        [Range(0, 1.0f)]
        public float Hardness = 0.7f;

        public bool DisableLineDrawing = false;

        [Header("Size modifiers")]
        [SerializeField]
        public bool EnableScalarSize;

        public bool EnableVelocitySize = true;

        [ShowIf("EnableVelocitySize")]
        public float MaxWidthVelocity = 2f;

        [ShowIf("EnableVelocitySize")]
        public float MinRatio = 0.4f;

        [ShowIf("EnableVelocitySize")]
        public bool InverseVelocitySize = true;

        public bool EnableStartSize = true;

        [ShowIf("EnableStartSize")]
        public float TimeLength = 0.2f;

        public bool EnableAngleSize = false;

        [Range(1, 90), ShowIf("EnableAngleSize")]
        public float MinAngle = 45.0f;

        [ShowIf("EnableAngleSize")]
        public float AngleSizeRatio = 3f;

        public bool EnableAbruptStopSize = false;

        [ShowIf("EnableAbruptStopSize")]
        public float VelocityThreshold = 5.0f;

        [ShowIf("EnableAbruptStopSize")]
        public float StopSizeScalar = 2.0f;

        public bool EnableAngleAbruptStopSize = false;

        [ShowIf("EnableAngleAbruptStopSize")]
        public float AngularVelocityThreshold = 5.0f;

        [ShowIf("EnableAngleAbruptStopSize")]
        public float AngleStopSizeScalar = 2.0f;

        [Header("Drip mode")]
        public bool EnableDripMode = false;

        [ShowIf("EnableDripMode")]
        public float SplatterSize = 2.0f;

        [ShowIf("EnableDripMode")]
        public float SplattersPerSecond = 10.0f;

        [Header("Drip modifiers")]
        public bool EnableDripVelocityFrequency = true;

        [ShowIf("EnableDripVelocityFrequency")]
        public float MaxFrequencyVelocity = 2f;

        [ShowIf("EnableDripVelocityFrequency")]
        public float FrequencyVelocityScalar = 2f;

        public bool EnableDripAngleFrequency = true;

        [ShowIf("EnableDripAngleFrequency")]
        public float MinAngularVelocity = 5f;

        [ShowIf("EnableDripAngleFrequency")]
        public float FrequencyAngleScalar = 2f;
    }
}
