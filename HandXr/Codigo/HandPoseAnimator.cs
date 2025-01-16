using UnityEngine;
using System.Collections;
using BIMOS;

public class HandPoseAnimator : MonoBehaviour
{
    public bool IsLeftHand;

    [SerializeField]
    private HandInputReader _handInputReader;

    public HandPose HandPose;
    [SerializeField]    

    public Transform[] Thumb, Index, Middle, Ring, Little;
    public bool IsIndexOnTrigger;

    private enum ThumbSubPoses
    {
        Idle,
        ThumbrestTouched,
        PrimaryTouched,
        PrimaryButton,
        SecondaryTouched,
        SecondaryButton,
        ThumbstickTouched
    }

    private ThumbSubPoses ThumbPose = ThumbSubPoses.Idle;

    private float IndexCurl, MiddleCurl, RingCurl, LittleCurl;

    private void Update()
    {
        if (!ValidateReferences())
        {
           // Debug.LogError("One or more required references are missing.");
            return;
        }

        UpdateCurls();
        UpdateHandPose();
    }
     private void LateUpdate()
        {
            
        }


    private bool ValidateReferences()
{
    if (_handInputReader == null)
    {
        Debug.LogError("HandInputReader is missing.");
        Debug.Log($"HandInputReader: {_handInputReader}");
        return false;
    }
    if (HandPose == null)
    {
        // Debug.LogError("HandPose is missing.");
        // Debug.Log($"HandPose: {HandPose}");
        return false;
    }
    if (Thumb == null || Thumb.Length != 3)
    {
        Debug.LogError("Thumb references are missing or incomplete.");
        Debug.Log($"Thumb: {Thumb?.Length}");
        return false;
    }
    if (Index == null || Index.Length != 3)
    {
        Debug.LogError("Index references are missing or incomplete.");
        Debug.Log($"Index: {Index?.Length}");
        return false;
    }
    if (Middle == null || Middle.Length != 3)
    {
        Debug.LogError("Middle references are missing or incomplete.");
        Debug.Log($"Middle: {Middle?.Length}");
        return false;
    }
    if (Ring == null || Ring.Length != 3)
    {
        Debug.LogError("Ring references are missing or incomplete.");
        Debug.Log($"Ring: {Ring?.Length}");
        return false;
    }
    if (Little == null || Little.Length != 3)
    {
        Debug.LogError("Little references are missing or incomplete.");
        Debug.Log($"Little: {Little?.Length}");
        return false;
    }

    return true;
}


    private void UpdateCurls()
    {
        // Determinar la subpose del pulgar basada en la entrada
        if (_handInputReader.SecondaryButton) ThumbPose = ThumbSubPoses.SecondaryButton;
        else if (_handInputReader.PrimaryButton) ThumbPose = ThumbSubPoses.PrimaryButton;
        else if (_handInputReader.SecondaryTouched) ThumbPose = ThumbSubPoses.SecondaryTouched;
        else if (_handInputReader.PrimaryTouched) ThumbPose = ThumbSubPoses.PrimaryTouched;
        else if (_handInputReader.ThumbstickTouched) ThumbPose = ThumbSubPoses.ThumbstickTouched;
        else if (_handInputReader.ThumbrestTouched) ThumbPose = ThumbSubPoses.ThumbrestTouched;
        else ThumbPose = ThumbSubPoses.Idle;

        // Determinar la curvatura de los dedos basada en la entrada
        IndexCurl = _handInputReader.Trigger;
        IsIndexOnTrigger = _handInputReader.TriggerTouched;
        MiddleCurl = RingCurl = LittleCurl = _handInputReader.Grip;
    }

    private void UpdateHandPose()
    {
        ApplyThumbPose();
       // ApplyFingerPose(Index, IndexCurl, HandPose.Index.Open, HandPose.Index.Closed);
        ApplyFingerPose(Middle, MiddleCurl, HandPose.Middle.Open, HandPose.Middle.Closed);
        ApplyFingerPose(Ring, RingCurl, HandPose.Ring.Open, HandPose.Ring.Closed);
        ApplyFingerPose(Little, LittleCurl, HandPose.Little.Open, HandPose.Little.Closed);
         if (IsIndexOnTrigger)
            {
                ApplyFingerPose(Index, IndexCurl, HandPose.Index.TriggerTouched, HandPose.Index.Closed);
            }
            else
            {
                ApplyFingerPose(Index, IndexCurl, HandPose.Index.Open, HandPose.Index.Closed);
            }
    }

    private void ApplyThumbPose()
    {
        FingerPose thumbPose = ThumbPose switch
        {
            ThumbSubPoses.Idle => HandPose.Thumb.Idle,
            ThumbSubPoses.ThumbrestTouched => HandPose.Thumb.ThumbrestTouched,
            ThumbSubPoses.PrimaryTouched => HandPose.Thumb.PrimaryTouched,
            ThumbSubPoses.PrimaryButton => HandPose.Thumb.PrimaryButton,
            ThumbSubPoses.SecondaryTouched => HandPose.Thumb.SecondaryTouched,
            ThumbSubPoses.SecondaryButton => HandPose.Thumb.SecondaryButton,
            ThumbSubPoses.ThumbstickTouched => HandPose.Thumb.ThumbstickTouched,
            _ => HandPose.Thumb.Idle
        };

        ApplyFingerPose(Thumb, 1f, thumbPose, thumbPose);
    }

    private void ApplyFingerPose(Transform[] finger, float curlValue, FingerPose openPose, FingerPose closedPose)
    {
        if (finger == null || finger.Length != 3) return;

        Transform root = finger[0];
        Transform middle = finger[1];
        Transform tip = finger[2];

        // if (IsLeftHand)
        // {
        //     openPose = openPose.Mirrored();
        //     closedPose = closedPose.Mirrored();
        // }

        root.localRotation = Quaternion.Slerp(openPose.RootBone, closedPose.RootBone, curlValue);
        middle.localRotation = Quaternion.Slerp(openPose.MiddleBone, closedPose.MiddleBone, curlValue);
        tip.localRotation = Quaternion.Slerp(openPose.TipBone, closedPose.TipBone, curlValue);
    }
}
