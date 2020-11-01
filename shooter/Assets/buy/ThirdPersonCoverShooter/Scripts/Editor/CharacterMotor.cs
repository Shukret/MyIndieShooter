using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(CharacterMotor))]
    [CanEditMultipleObjects]
    public class CharacterMotorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Auto IK Setup"))
            {
                Undo.RecordObjects(targets, "IK Setup");

                foreach (var object_ in targets)
                {
                    var motor = (CharacterMotor)object_;
                    var animator = motor.GetComponent<Animator>();

                    var settings = motor.IK;

                    var spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                    var lowerSpine = spine.GetChild(0);

                    settings.AimChain.Bones = new IKBone[3];
                    settings.AimChain.Bones[0] = new IKBone(spine, 0.5f);
                    settings.AimChain.Bones[1] = new IKBone(lowerSpine, 0.5f);
                    settings.AimChain.Bones[2] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightShoulder), animator.GetBoneTransform(HumanBodyBones.LeftShoulder), 0.8f);

                    settings.ThrowChain.Bones = new IKBone[2];
                    settings.ThrowChain.Bones[0] = new IKBone(spine, 0.5f);
                    settings.ThrowChain.Bones[1] = new IKBone(lowerSpine, 0.5f);

                    settings.SightChain.Bones = new IKBone[2];
                    settings.SightChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.Neck), 0.5f);
                    settings.SightChain.Bones[1] = new IKBone(animator.GetBoneTransform(HumanBodyBones.Head), 0.8f);

                    settings.LeftArmChain.Bones = new IKBone[3];
                    settings.LeftArmChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.LeftShoulder), 0.5f);
                    settings.LeftArmChain.Bones[1] = new IKBone(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), 0.5f);
                    settings.LeftArmChain.Bones[2] = new IKBone(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), 0.8f);

                    settings.RecoilChain.Bones = new IKBone[4];
                    settings.RecoilChain.Bones[0] = new IKBone(lowerSpine, 0.25f);
                    settings.RecoilChain.Bones[1] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightShoulder), 0.5f);
                    settings.RecoilChain.Bones[2] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), 0.8f);
                    settings.RecoilChain.Bones[3] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), 0.5f);

                    settings.LeftHand = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                    settings.RightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    settings.HitBone = spine;

                    var head = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (settings.Sight == null) settings.Sight = head.Find("Look");
                    if (settings.Sight == null) settings.Sight = head.Find("Sight");
                    if (settings.Sight == null) settings.Sight = head.Find("look");
                    if (settings.Sight == null) settings.Sight = head.Find("sight");

                    motor.IK = settings;
                }
            }

            if (GUILayout.Button("Minimal IK Setup"))
            {
                Undo.RecordObjects(targets, "IK Setup");

                foreach (var object_ in targets)
                {
                    var motor = (CharacterMotor)object_;
                    var animator = motor.GetComponent<Animator>();

                    var settings = motor.IK;

                    var spine = animator.GetBoneTransform(HumanBodyBones.Spine);

                    settings.AimChain.Iterations = 1;
                    settings.AimChain.Bones = new IKBone[1];
                    settings.AimChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightShoulder), animator.GetBoneTransform(HumanBodyBones.LeftShoulder), 0.8f);

                    settings.ThrowChain.Iterations = 1;
                    settings.ThrowChain.Bones = new IKBone[2];
                    settings.ThrowChain.Bones[0] = new IKBone(spine, 1.0f);

                    settings.SightChain.Iterations = 1;
                    settings.SightChain.Bones = new IKBone[1];
                    settings.SightChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.Neck), 1.0f);

                    settings.LeftArmChain.Iterations = 1;
                    settings.LeftArmChain.Bones = new IKBone[1];
                    settings.LeftArmChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), 1.0f);

                    settings.RecoilChain.Bones = new IKBone[1];
                    settings.RecoilChain.Iterations = 1;
                    settings.RecoilChain.Bones[0] = new IKBone(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), 0.5f);

                    settings.LeftHand = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                    settings.RightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    settings.HitBone = spine;

                    var head = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (settings.Sight == null) settings.Sight = head.Find("Look");
                    if (settings.Sight == null) settings.Sight = head.Find("Sight");
                    if (settings.Sight == null) settings.Sight = head.Find("look");
                    if (settings.Sight == null) settings.Sight = head.Find("sight");

                    motor.IK = settings;
                }
            }

            if (GUILayout.Button("Default IK Delay"))
            {
                Undo.RecordObjects(targets, "Default IK Delay");

                foreach (var object_ in targets)
                {
                    var motor = (CharacterMotor)object_;
                    var settings = motor.IK;

                    {
                        var bones = settings.AimChain.Bones;
                        settings.AimChain = IKChain.Default();
                        settings.AimChain.Bones = bones;
                    }

                    {
                        var bones = settings.ThrowChain.Bones;
                        settings.ThrowChain = IKChain.Default();
                        settings.ThrowChain.Bones = bones;
                    }

                    {
                        var bones = settings.SightChain.Bones;
                        settings.SightChain = IKChain.Default();
                        settings.SightChain.Bones = bones;
                    }

                    {
                        var bones = settings.LeftArmChain.Bones;
                        settings.LeftArmChain = IKChain.Default();
                        settings.LeftArmChain.Bones = bones;
                    }

                    {
                        var bones = settings.RecoilChain.Bones;
                        settings.RecoilChain = IKChain.Default();
                        settings.RecoilChain.Bones = bones;
                    }

                    motor.IK = settings;
                }
            }
        }
    }
}
