using System;
using System.Collections.Generic;
using UnityEngine;

using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

 public class PoseEditor : MonoBehaviour
    {
        [Serializable]
        public struct FingerTransforms
        {
            public Transform RootBone;
            public Transform MiddleBone;
            public Transform TipBone;
        }

        public HandPose currentPose;
        public string label = "DefaultPose"; // Etiqueta predeterminada para subposes

        public FingerTransforms ThumbTransforms;
        public FingerTransforms IndexTransforms;
        public FingerTransforms MiddleTransforms;
        public FingerTransforms RingTransforms;
        public FingerTransforms LittleTransforms;

        public enum FingerType { Thumb, Index, Middle, Ring, Little }
        public FingerType SelectedFinger;

        public enum SubPoseType { Idle, Open, Closed, TriggerTouched, PrimaryTouched, PrimaryButton, SecondaryTouched, SecondaryButton, ThumbstickTouched,ThumbrestTouched }
        public SubPoseType SelectedSubPose;

        [ContextMenu("Save SubPose")]
        public void SaveSubPose()
        {
            if (currentPose == null)
            {
                Debug.LogError("Current pose is not assigned.");
                return;
            }

            FingerTransforms selectedTransforms = GetFingerTransforms(SelectedFinger);

            if (selectedTransforms.RootBone == null || selectedTransforms.MiddleBone == null || selectedTransforms.TipBone == null)
            {
                Debug.LogError("One or more transforms for the selected finger are not assigned.");
                return;
            }

            FingerPose newPose = CreateFingerPose(selectedTransforms);
            AssignFingerPose(newPose, SelectedFinger, SelectedSubPose);

            #if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(currentPose);
            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = "Assets/HandPoses/" + currentPose.name + ".asset";
                AssetDatabase.CreateAsset(currentPose, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(currentPose);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SubPose saved: " + SelectedFinger + " - " + SelectedSubPose);
            #endif
        }

        private FingerTransforms GetFingerTransforms(FingerType fingerType)
        {
            return fingerType switch
            {
                FingerType.Thumb => ThumbTransforms,
                FingerType.Index => IndexTransforms,
                FingerType.Middle => MiddleTransforms,
                FingerType.Ring => RingTransforms,
                FingerType.Little => LittleTransforms,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void AssignFingerPose(FingerPose pose, FingerType fingerType, SubPoseType subPoseType)
        {
            switch (fingerType)
            {
                case FingerType.Thumb:
                    AssignThumbPose(pose, subPoseType);
                    break;
                case FingerType.Index:
                    AssignIndexPose(pose, subPoseType);
                    break;
                case FingerType.Middle:
                    AssignGenericFingerPose(pose, currentPose.Middle, subPoseType);
                    break;
                case FingerType.Ring:
                    AssignGenericFingerPose(pose, currentPose.Ring, subPoseType);
                    break;
                case FingerType.Little:
                    AssignGenericFingerPose(pose, currentPose.Little, subPoseType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AssignThumbPose(FingerPose pose, SubPoseType subPoseType)
        {
            switch (subPoseType)
            {
                case SubPoseType.Idle:
                    currentPose.Thumb.Idle = pose;
                    break;
                case SubPoseType.PrimaryTouched:
                    currentPose.Thumb.PrimaryTouched = pose;
                    break;
                case SubPoseType.PrimaryButton:
                    currentPose.Thumb.PrimaryButton = pose;
                    break;
                case SubPoseType.SecondaryTouched:
                    currentPose.Thumb.SecondaryTouched = pose;
                    break;
                case SubPoseType.SecondaryButton:
                    currentPose.Thumb.SecondaryButton = pose;
                    break;
                case SubPoseType.ThumbstickTouched:
                    currentPose.Thumb.ThumbstickTouched = pose;
                    break;
                case SubPoseType.ThumbrestTouched:
                    currentPose.Thumb.ThumbrestTouched = pose;
                    break;
                default:
                    Debug.LogError("Invalid SubPoseType for Thumb.");
                    break;
            }
        }

        private void AssignIndexPose(FingerPose pose, SubPoseType subPoseType)
        {
            switch (subPoseType)
            {
                case SubPoseType.Open:
                    currentPose.Index.Open = pose;
                    break;
                case SubPoseType.Closed:
                    currentPose.Index.Closed = pose;
                    break;
                case SubPoseType.TriggerTouched:
                    currentPose.Index.TriggerTouched = pose;
                    break;
                default:
                    Debug.LogError("Invalid SubPoseType for Index.");
                    break;
            }
        }

        private void AssignGenericFingerPose(FingerPose pose, FingerPoses fingerPoses, SubPoseType subPoseType)
        {
            switch (subPoseType)
            {
                case SubPoseType.Open:
                    fingerPoses.Open = pose;
                    break;
                case SubPoseType.Closed:
                    fingerPoses.Closed = pose;
                    break;
                default:
                    Debug.LogError("Invalid SubPoseType for Generic Finger.");
                    break;
            }
        }

        private FingerPose CreateFingerPose(FingerTransforms transforms)
        {
            Debug.Log($"Saving FingerPose: RootBone {transforms.RootBone.localRotation}, MiddleBone {transforms.MiddleBone.localRotation}, TipBone {transforms.TipBone.localRotation}");
            return new FingerPose
            {
                RootBone = transforms.RootBone.localRotation,
                MiddleBone = transforms.MiddleBone.localRotation,
                TipBone = transforms.TipBone.localRotation
            };
        }

        [ContextMenu("Apply SubPose")]
        public void ApplySubPose()
        {
            if (currentPose == null)
            {
                Debug.LogError("Current pose is not assigned.");
                return;
            }

            FingerTransforms selectedTransforms = GetFingerTransforms(SelectedFinger);

            if (selectedTransforms.RootBone == null || selectedTransforms.MiddleBone == null || selectedTransforms.TipBone == null)
            {
                Debug.LogError("One or more transforms for the selected finger are not assigned.");
                return;
            }

            FingerPose pose = GetFingerPose(SelectedFinger, SelectedSubPose);

            ApplyFingerPose(pose, selectedTransforms);
            Debug.Log("SubPose applied: " + SelectedFinger + " - " + SelectedSubPose);
        }

        private FingerPose GetFingerPose(FingerType fingerType, SubPoseType subPoseType)
        {
            return fingerType switch
            {
                FingerType.Thumb => GetThumbPose(subPoseType),
                FingerType.Index => GetIndexPose(subPoseType),
                FingerType.Middle => GetGenericFingerPose(currentPose.Middle, subPoseType),
                FingerType.Ring => GetGenericFingerPose(currentPose.Ring, subPoseType),
                FingerType.Little => GetGenericFingerPose(currentPose.Little, subPoseType),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private FingerPose GetThumbPose(SubPoseType subPoseType)
        {
            return subPoseType switch
            {
                SubPoseType.Idle => currentPose.Thumb.Idle,
                SubPoseType.PrimaryTouched => currentPose.Thumb.PrimaryTouched,
                SubPoseType.PrimaryButton => currentPose.Thumb.PrimaryButton,
                SubPoseType.SecondaryTouched => currentPose.Thumb.SecondaryTouched,
                SubPoseType.SecondaryButton => currentPose.Thumb.SecondaryButton,
                SubPoseType.ThumbstickTouched => currentPose.Thumb.ThumbstickTouched,
                SubPoseType.ThumbrestTouched => currentPose.Thumb.ThumbrestTouched,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private FingerPose GetIndexPose(SubPoseType subPoseType)
        {
            return subPoseType switch
            {
                SubPoseType.Open => currentPose.Index.Open,
                SubPoseType.Closed => currentPose.Index.Closed,
                SubPoseType.TriggerTouched => currentPose.Index.TriggerTouched,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private FingerPose GetGenericFingerPose(FingerPoses fingerPoses, SubPoseType subPoseType)
        {
            return subPoseType switch
            {
                SubPoseType.Open => fingerPoses.Open,
                SubPoseType.Closed => fingerPoses.Closed,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void ApplyFingerPose(FingerPose pose, FingerTransforms transforms)
        {
            if (transforms.RootBone != null) transforms.RootBone.localRotation = pose.RootBone;
            if (transforms.MiddleBone != null) transforms.MiddleBone.localRotation = pose.MiddleBone;
            if (transforms.TipBone != null) transforms.TipBone.localRotation = pose.TipBone;
        }
    }
