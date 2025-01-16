using System.Collections;
using UnityEngine;

public class WeaponSlide : MonoBehaviour
{
    
    
        /// <summary>
        /// Minimum distance slide will travel on Z axis
        /// </summary>
        public float MinLocalZ = -0.03f;

        /// <summary>
        /// Max distance slide will travel on Z axis
        /// </summary>
        public float MaxLocalZ = 0;

        // Keep track of which way we are sliding
        public bool slidingBack = true;

        /// <summary>
        /// Is the Slide locked back due to last shot
        /// </summary>
        public bool LockedBack = false;

        /// <summary>
        /// Sound to play when slide is released back into position
        /// </summary>
        public AudioClip SlideReleaseSound;

        /// <summary>
        /// Sound to play after last shot has fired and slide is forced back
        /// </summary>
        public AudioClip LockedBackSound;

        public bool Bool_onSlideBack= false;   
        public bool SliderEnManualMode=false;    
         Vector3 initialLocalPos;
        
        AudioSource audioSource;
         /// <summary>
        /// Lock the slide position in place
        /// </summary>
        Vector3 _lockPosition;

         bool lockSlidePosition;


         public bool IsGrabbinSlider = false;
         public bool IsGrabbingGun = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

         initialLocalPos = transform.localPosition;
         audioSource = GetComponent<AudioSource>();
        
    }

    // Update is called once per frame
     void Update() {

            // If our slide is currently locked just set it and return early
            if(lockSlidePosition) {
                transform.localPosition = _lockPosition;
                return;
            }

            float localZ = transform.localPosition.z;

            if (LockedBack) {
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);

                // Not locking back if hand is holding this
                if (IsGrabbinSlider) {
                    UnlockBack();
                }
            }

             if (IsGrabbinSlider) {
                    SliderEnManualMode=true;    
                }
            else{
                    SliderEnManualMode=false;   
                }

            if (!LockedBack) {
                // Clamp values
                if (localZ <= MinLocalZ) {
                    transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);
                    if (slidingBack) {
                        onSlideBack();
                        
                    }
                }
                else if (localZ >= MaxLocalZ) {
                    transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MaxLocalZ);

                    // Moving forward
                    if (!slidingBack) {
                        onSlideForward();
                       
                    }
                }
            }
        }
          public virtual void LockBack() {

            if (!LockedBack) {
                if (IsGrabbinSlider|| IsGrabbingGun) {
                 //   VRUtils.Instance.PlaySpatialClipAt(LockedBackSound, transform.position, 1f, 0.8f);

                
                }
               Debug.Log("LockBack");
               

                LockedBack = true;
            }
        }

        public virtual void UnlockBack() {

            if (LockedBack) {
                if (IsGrabbinSlider || IsGrabbingGun) {
                 //   VRUtils.Instance.PlaySpatialClipAt(SlideReleaseSound, transform.position, 1f, 0.9f);
                }

                LockedBack = false;

               // This is considered a charge
                // if (parentWeapon != null) {
                //     parentWeapon.OnWeaponCharged(false);
                // }
            }
        }

        void onSlideBack() {

            if (IsGrabbinSlider || IsGrabbingGun) {
                playSoundInterval(0, 0.2f, 0.9f);
                Bool_onSlideBack=true;
                
               
            }

            

            slidingBack = false;
        }

        void onSlideForward() {

            if (IsGrabbinSlider|| IsGrabbingGun) {
                playSoundInterval(0.2f, 0.35f, 1f);
            }

           Bool_onSlideBack=false;

            slidingBack = true;
        }

        public virtual void LockSlidePosition() {
            // Lock the slide position if we aren't holding the object
            if (IsGrabbingGun && !IsGrabbinSlider && !lockSlidePosition) {
                _lockPosition = transform.localPosition;
                lockSlidePosition = true;
            }
        }

        public virtual void UnlockSlidePosition() {
            if (lockSlidePosition) {
                StartCoroutine(UnlockSlideRoutine());
            }
        }

        public IEnumerator UnlockSlideRoutine() {
            yield return new WaitForSeconds(0.2f);
            lockSlidePosition = false;
        }

        void playSoundInterval(float fromSeconds, float toSeconds, float volume) {
            if (audioSource) {

                if (audioSource.isPlaying) {
                    audioSource.Stop();
                }

                audioSource.pitch = Time.timeScale;
                audioSource.time = fromSeconds;
                audioSource.volume = volume;
                audioSource.Play();
                audioSource.SetScheduledEndTime(AudioSettings.dspTime + (toSeconds - fromSeconds));
            }
        }

        public void IsGrabbingGunVoid()
        {

            IsGrabbingGun = true;


        }
        public void IsNotGrabbingGunVoid()
        {
            IsGrabbingGun = false;


        }
        public void IsGrabbingSliderVoid()
        {

            IsGrabbinSlider = true;


        }
        public void IsNotGrabbingSliderVoid()
        {
            IsGrabbinSlider = false;


        }
}
