﻿namespace Logitech.XRToolkit.Triggers
{
    using Logitech.XRToolkit.Core;
    using System;
    using UnityEngine;

    /// <summary>
    /// Trigger that is valid when a the target transform shakes.
    /// </summary>
    [Serializable]
    public class ShakeTrigger : Trigger
    {
        [SerializeField]
        private Transform _source;
        [SerializeField]
        private Vector3 _positionLastFrame, _lastMovementDirection;
        private float _lastMovementChangeTime = 0f;
        private int _shakeCount = 0;

        // Movements under X m/s are disregarded.
        private const float VelocityThreshold = 2f;
        // One 'shake' every X seconds maximum.
        private const float MaxDeltaTime = 0.05f;
        // X shakes to trigger.
        private const int ShakeCountThreshold = 4;

        public override bool IsValid()
        {
            var positionThisFrame = _source.position;
            var movement = positionThisFrame - _positionLastFrame;
            var velocity = movement.magnitude / Time.deltaTime;

            // If we go fast enough.
            if (velocity > VelocityThreshold)
            {
                // If we changed direction.
                if (Vector3.Dot(movement.normalized, _lastMovementDirection) > 0f)
                {
                    // If we did so not too long ago.
                    if (Time.time - _lastMovementChangeTime < MaxDeltaTime)
                    {
                        _shakeCount++;
                    }
                    // Set the current time as the new point in time to compare against.
                    _lastMovementChangeTime = Time.time;
                }
                _lastMovementDirection = movement.normalized;
            }
            _positionLastFrame = positionThisFrame;

            // If we just reached max count then we can trigger and reset our count.
            if (_shakeCount >= ShakeCountThreshold)
            {
                _shakeCount = 0;
                return true;
            }

            return false;
        }
    }
}
