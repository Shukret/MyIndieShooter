using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Representation of a transform manipulated by IK.
    /// </summary>
    public class IKTransform
    {
        /// <summary>
        /// Local accumulated change in rotation.
        /// </summary>
        public Quaternion Change = Quaternion.identity;

        /// <summary>
        /// Link to the Transform that IKTransform represents.
        /// </summary>
        public Transform Link;

        /// <summary>
        /// Link to the parent transform representation.
        /// </summary>
        public IKTransform Parent;

        /// <summary>
        /// Link to the child transform representation.
        /// </summary>
        public IKTransform Child;

        /// <summary>
        /// Current calculated position in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Current calculated rotation in world space.
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Saved position in world space of the linked bone.
        /// </summary>
        public Vector3 SavedPosition;

        /// <summary>
        /// Saved rotation in world space of the linked bone.
        /// </summary>
        public Quaternion SavedRotation;

        /// <summary>
        /// Saved forward vector in world space of the linked bone.
        /// </summary>
        public Vector3 SavedForward;

        /// <summary>
        /// Saved position in local space of the linked bone.
        /// </summary>
        public Vector3 SavedLocalPosition;

        private Quaternion _storedTotalChange;

        /// <summary>
        /// Prepare the transform for manipulation.
        /// </summary>
        public void Reset(Transform link)
        {
            Change = Quaternion.identity;
            Link = link;
            SavedPosition = Link.position;
            SavedRotation = Link.rotation;
            SavedForward = Link.forward;

            if (link.parent != null)
                SavedLocalPosition = Quaternion.Inverse(link.parent.rotation) * (link.parent.TransformPoint(link.localPosition) - link.parent.position);

            Parent = null;
            Child = null;
        }

        /// <summary>
        /// Calculate position and rotation for the transform and recursively call child transforms.
        /// </summary>
        public void Calc()
        {
            if (Parent == null)
                _storedTotalChange = Change;
            else
                _storedTotalChange = Parent._storedTotalChange * Change;

            if (Parent == null)
                Position = SavedPosition;
            else
                Position = Parent.Position + Parent.Rotation * SavedLocalPosition;

            Rotation = _storedTotalChange * SavedRotation;

            if (Child != null)
                Child.Calc();
        }

        /// <summary>
        /// Forward vector of the transform.
        /// </summary>
        public Vector3 Forward
        {
            get { return _storedTotalChange * SavedForward; }
        }
    }

    /// <summary>
    /// Object that calculates IK transformations.
    /// </summary>
    public class IK
    {
        /// <summary>
        /// Current object to base IK transformations on.
        /// </summary>
        public Transform Target;

        /// <summary>
        /// Chain of bones that are manipulated.
        /// </summary>
        public IKBone[] Bones;

        private IKTransform _target;
        private IKTransform[] _transforms = new IKTransform[16];
        private float _updateTime;

        public IK()
        {
            for (int i = 0; i < _transforms.Length; i++)
                _transforms[i] = new IKTransform();
        }

        /// <summary>
        /// Manipulates bones till the Target is looking towards the given target position.
        /// Store results and update again only after a certain amount of time has passed.
        /// </summary>
        public void UpdateAim(Vector3 targetPosition, float delay, float weight, int iterations)
        {
            if (Time.realtimeSinceStartup - _updateTime >= delay)
            {
                CalcAim(targetPosition, iterations);
                _updateTime = Time.realtimeSinceStartup;
            }

            AssignTransforms(weight);
        }

        /// <summary>
        /// Manipulates bones till the Target is at the given target position.
        /// Store results and update again only after a certain amount of time has passed.
        /// </summary>
        public void UpdateMove(Vector3 targetPosition, float delay, float weight, int iterations)
        {
            if (Time.realtimeSinceStartup - _updateTime >= delay)
            {
                CalcMove(targetPosition, iterations);
                _updateTime = Time.realtimeSinceStartup;
            }

            AssignTransforms(weight);
        }

        /// <summary>
        /// Manipulates bones till the Target is looking towards the given target position.
        /// </summary>
        /// <param name="targetPosition">Position to aim at.</param>
        /// <param name="iterations">Number of iterations.</param>
        public void CalcAim(Vector3 targetPosition, int iterations)
        {
            if (!prepareTransforms())
                return;

            for (int i = 0; i < iterations; i++)
            {
                for (int b = 0; b < Bones.Length - 1; b++)
                    solveAimBone(targetPosition, Bones[b], (i + 1) / (float)Bones.Length);

                solveAimBone(targetPosition, Bones[Bones.Length - 1], 1.0f);
            }
        }

        /// <summary>
        /// Manipulates bones till the Target is at the given target position.
        /// </summary>
        /// <param name="targetPosition">Position to move Targe to.</param>
        /// <param name="iterations">Number of iterations.</param>
        public void CalcMove(Vector3 targetPosition, int iterations)
        {
            if (!prepareTransforms())
                return;

            for (int i = 0; i < iterations; i++)
            {
                for (int b = 0; b < Bones.Length - 1; b++)
                    solveMoveBone(targetPosition, Bones[b], (i + 1) / (float)Bones.Length);

                solveMoveBone(targetPosition, Bones[Bones.Length - 1], 1.0f);
            }
        }

        /// <summary>
        /// Calculates transformation of a single bone, used to aim Target to a target position.
        /// </summary>
        private void solveAimBone(Vector3 targetPosition, IKBone bone, float weightMultiplier = 1.0f)
        {
            if (bone.Link == null)
                return;

            var weight = bone.Weight * weightMultiplier;
            var offset = Quaternion.FromToRotation(_target.Forward, (targetPosition - _target.Position).normalized);

            bone.Link.Change = Quaternion.Lerp(bone.Link.Change, offset * bone.Link.Change, weight);
            bone.Link.Calc();
        }

        /// <summary>
        /// Calculates transformation of a single bone, used to move Target to a target position.
        /// </summary>
        private void solveMoveBone(Vector3 targetPosition, IKBone bone, float weightMultiplier = 1.0f)
        {
            if (bone.Link == null)
                return;

            var weight = bone.Weight * weightMultiplier;
            var current = bone.Link.Position;
            var offset = Quaternion.FromToRotation((_target.Position - current).normalized, (targetPosition - current).normalized);

            bone.Link.Change = Quaternion.Lerp(bone.Link.Change, offset * bone.Link.Change, weight);
            bone.Link.Calc();
        }

        /// <summary>
        /// Assign calculated transformations to bones. 
        /// </summary>
        private void AssignTransforms(float weight)
        {
            for (int i = Bones.Length - 1; i >= 0; i--)
            {
                var bone = Bones[i];

                if (bone.Transform != null && bone.Link != null)
                    bone.Transform.rotation = Quaternion.Lerp(bone.Transform.rotation, bone.Link.Change * bone.Transform.rotation, weight);

                if (bone.Sibling != null)
                    bone.Sibling.rotation = Quaternion.Lerp(bone.Sibling.rotation, bone.Link.Change * bone.Sibling.rotation, weight);
            }
        }

        /// <summary>
        /// Remember original transformations of all bones.
        /// </summary>
        private bool prepareTransforms()
        {
            if (Bones.Length == 0 || Target == null)
                return false;

            for (int i = 0; i < Bones.Length; i++)
                Bones[i].Link = null;

            var transformIndex = 0;

            _target = _transforms[transformIndex++];
            _target.Reset(Target);

            int lastBone = Bones.Length;
            findBone(_target, ref lastBone);

            var transform = Target.parent;
            var current = _target;

            while (transform != null && lastBone > 0)
            {
                var parentNode = _transforms[transformIndex++];
                parentNode.Reset(transform);

                findBone(parentNode, ref lastBone);

                current.Parent = parentNode;
                parentNode.Child = current;

                transform = transform.parent;
                current = parentNode;
            }

            current.Calc();

            return true;
        }

        private void findBone(IKTransform transform, ref int last)
        {
            for (int i = last - 1; i >= 0; i--)
                if (transform.Link == Bones[i].Transform)
                {
                    Bones[i].Link = transform;
                    last = i;
                    break;
                }
        }
    }
}