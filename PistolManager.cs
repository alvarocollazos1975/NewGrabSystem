using System.Collections;
using BIMOS;
using BIMOS.Samples;
using UnityEngine;
using UnityEngine.Events;

public class PistolManager : MonoBehaviour
{
    [Header("Weapon Setup")]
    [SerializeField] private ArticulationBody _pistolBody;
    [SerializeField] private ArticulationBody _slideBody;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Transform _barrelTransform;
    [SerializeField] private float recoilMultiplier;
      [SerializeField] private float PowerMultiplier;
    [SerializeField] private Rigidbody _cartridgePrefab;
    [SerializeField] private Rigidbody _cartridgeSinCAbezaPrefab;
    public GameObject casingPrefab;
    [SerializeField] private Transform _cartridgeSpawnTransform;
    [SerializeField] private Vector3 _ejectForce;
    [SerializeField] private ParticleSystem _muzzleFlash;
    [SerializeField] private Renderer bulletRenderer;
    [SerializeField] private Renderer casingRenderer;
    [SerializeField] private GameObject magazinePrefab;
    [SerializeField] private Socket _magazineSocket;
    [SerializeField] private GameObject _bulletHolePrefab; // Make an object pooling system if you want

     public enum ReloadType {
        InfiniteAmmo,
        ManualClip,
        InternalAmmo
    }
     /// <summary>
        /// How does the user reload once the Clip is Empty
        /// </summary>
        public ReloadType ReloadMethod = ReloadType.InfiniteAmmo;

     [SerializeField] private int _CAntidadeBalas=16;
        [Header("Settings")]
    // [SerializeField] private float destroyTimer = 2f;
   
    // [SerializeField] private float ejectPower = 150f;

    public bool _CargadorInsertado = false;
     private enum CartridgeStatus { None, ToChamber, Chambered, ToEject }
    [SerializeField] private CartridgeStatus _cartridgeStatus = CartridgeStatus.None;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] _gunshots;
    [SerializeField] private AudioClip[] _triggerPulls;
    [SerializeField] private AudioClip[] _slideSounds;
    [SerializeField] private AudioClip[] _impacts;

    [Header("Weapon Events")]
    public UnityEvent OnFire;
    public UnityEvent OnReload;
    public UnityEvent OnSlideLock;   

   

    private bool _isSlideLocked = false;
    private bool _isBulletChambered = false;

    private void Awake()
    {
       
        
    }
    private void Update()
    {
        HandleSlideLock();
        
    }

    private void  FixedUpdate()
    {
       

        

    }

    public void Fire()
{
    if (!CanFire())
    {
        PlaySound(_triggerPulls);
        return;
    }

    // Dispara el arma
    PlaySound(_gunshots);
    RemoveBullet();
    
    _isBulletChambered = false;
    bulletRenderer.enabled = false;  
       //bulletRenderer.enabled = false;
    casingRenderer.enabled = false;  

    // Simula el retroceso
    _slideBody.AddForceAtPosition(-_barrelTransform.forward * recoilMultiplier, _barrelTransform.position, ForceMode.Impulse);
    EjectCasingEntiro();
    _muzzleFlash.Play();

    // Maneja el impacto del disparo
    HandleBulletImpact();

    OnFire?.Invoke();

}


    private void HandleBulletImpact()
    {
        RaycastHit hit;
            if (Physics.Raycast(_barrelTransform.position, _barrelTransform.forward, out hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!hit.transform)
                    return;

                //Damage hit
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(10);
                }

                //Fling hit
                if (hit.rigidbody)
                    hit.rigidbody.AddForceAtPosition(_barrelTransform.forward * PowerMultiplier, hit.point, ForceMode.Impulse);

                if (hit.articulationBody)
                    hit.articulationBody.AddForceAtPosition(_barrelTransform.forward * PowerMultiplier, hit.point, ForceMode.Impulse);

                //Add bullet hole
                GameObject bulletHole = Instantiate(_bulletHolePrefab, hit.point - _barrelTransform.forward * 0.1f, Quaternion.LookRotation(_barrelTransform.forward), hit.transform);
                bulletHole.transform.Rotate(Vector3.forward, Random.Range(0, 360));
                bulletHole.transform.GetChild(0).transform.forward = hit.normal;
                bulletHole.GetComponent<AudioSource>().PlayOneShot(Utilities.RandomAudioClip(_impacts));
                Destroy(bulletHole, 5f);
            }
    }
     public void OnGrab()
        {
            if (FindAnyObjectByType<AmmoPouch>())
                FindAnyObjectByType<AmmoPouch>().MagazinePrefab = magazinePrefab;
        }

    public void SlideBack()
    {
         Magazine magazine = _magazineSocket.Attacher?.Rigidbody.GetComponent<Magazine>();
        if (_cartridgeStatus == CartridgeStatus.Chambered && _CAntidadeBalas>0)
        {  
           if (magazine != null && magazine.BalaMovidaTochamber && _CAntidadeBalas>0)
           {
             magazine.BalaMovidaTochamber=false;
             RemoveBullet();
             EjectCasing();
              bulletRenderer.enabled = false;
            casingRenderer.enabled = false;
           }
           else if (magazine != null && !magazine.BalaMovidaTochamber && _CAntidadeBalas>0)
           {
             magazine.MoveRoundToChamber();
              bulletRenderer.enabled = true;
            casingRenderer.enabled = true;
           }
           
        }
        else  if (_cartridgeStatus == CartridgeStatus.ToChamber && _CAntidadeBalas>0 && magazine != null)
        {
            magazine.MoveRoundToChamber();
             bulletRenderer.enabled = true;
            casingRenderer.enabled = true;


        }

         

        if (_CargadorInsertado)
        {
            _cartridgeStatus = CartridgeStatus.ToChamber;
        }
        else{
            //Debug.Log("Cargador vacío o no presente SlideBack.");
        }

        PlaySound(_slideSounds);
    }

    public void SlideForward()
    {
        if (_isSlideLocked)
        {
            OnSlideLock?.Invoke();
            return;
        }      
        
         LoadNextRound();
    }

   public void ReleaseSlide()
{
    if (!_isSlideLocked) {
        return;
        }

    _isSlideLocked = false;
    SetSlideLock(false);
    TransitRound();
    PlaySound(_slideSounds);
   // Debug.Log("Deslizador liberado manualmente.");
}

     public void ReleaseMagazine()
        {
            if (!_magazineSocket.Attacher)
                return;

            _magazineSocket.Detach();
        }

   private void LoadNextRound()
{
    if (_cartridgeStatus != CartridgeStatus.ToChamber)
    {
       // Debug.Log("No hay balas para cargar en la recámara.");
        return;
    }

    Magazine magazine = _magazineSocket.Attacher?.Rigidbody.GetComponent<Magazine>();    

    if (magazine == null)
    {
       // Debug.Log("Cargador vacío o no presente.");
        _cartridgeStatus = CartridgeStatus.None;
        return;
    }

    _CAntidadeBalas = GetBulletCount();
    // Actualizar estado y habilitar representaciones gráficas
    _cartridgeStatus = CartridgeStatus.Chambered;
    _isBulletChambered = true;
   
}


   public virtual int GetBulletCount() {
            // int internalMunicion=16;
            
            if (ReloadMethod == ReloadType.InfiniteAmmo) {
                return 999;
            }
            else if (ReloadMethod == ReloadType.InternalAmmo) {
                return 16;
            }
            else if (ReloadMethod == ReloadType.ManualClip) {

                 Magazine magazine = _magazineSocket.Attacher?.Rigidbody.GetComponent<Magazine>();

                if (magazine == null || magazine.RemainingRounds <= 0)
                {
                    return 0;
                }
                else
                {
                    return magazine.RemainingRounds;
                }                
            }

            // Default to bullet count
            return 999;
        }

        public virtual void RemoveBullet() {

            // Don't remove bullet here
            if (ReloadMethod == ReloadType.InfiniteAmmo) {
                return;
            }

            else if (ReloadMethod == ReloadType.InternalAmmo) {
                _CAntidadeBalas--;
            }
            else if (ReloadMethod == ReloadType.ManualClip) {
                Magazine magazine = _magazineSocket.Attacher?.Rigidbody.GetComponent<Magazine>();
                // Deactivate gameobject as this bullet has been consumed
                if (magazine.RemainingRounds>0) {
                    magazine.RemoveActiveRound();
                    _CAntidadeBalas--;
                }
                else
                {
                     //Debug.Log("No balas en el cargador.....");

                }
            }

        }

        void updateChamberedBullet() {
            if (bulletRenderer != null) {
                bulletRenderer.gameObject.SetActive(bulletRenderer);
            }
        }




     private void EjectCasing()
        {
            Rigidbody cartridgeRigidbody = Instantiate(_cartridgePrefab, _cartridgeSpawnTransform.position, _cartridgeSpawnTransform.rotation);
            
            cartridgeRigidbody.linearVelocity = _pistolBody.linearVelocity;
            cartridgeRigidbody.angularVelocity = _pistolBody.angularVelocity;
            cartridgeRigidbody.AddRelativeForce(_ejectForce);
            Destroy(cartridgeRigidbody.gameObject, 5);
        }
        private void EjectCasingEntiro()
        {
            Rigidbody cartridgeRigidbody = Instantiate(_cartridgeSinCAbezaPrefab, _cartridgeSpawnTransform.position, _cartridgeSpawnTransform.rotation);
            
            cartridgeRigidbody.linearVelocity = _pistolBody.linearVelocity;
            cartridgeRigidbody.angularVelocity = _pistolBody.angularVelocity;
            cartridgeRigidbody.AddRelativeForce(_ejectForce);
            Destroy(cartridgeRigidbody.gameObject, 5);
        }

   private void HandleSlideLock()
{
    Magazine magazine = _magazineSocket.Attacher?.GetComponent<Magazine>();

    if (_cartridgeStatus == CartridgeStatus.None && (magazine == null || _CAntidadeBalas <= 0))
    {
        _isSlideLocked = true;
        SetSlideLock(true);
    }
    else if (_cartridgeStatus == CartridgeStatus.Chambered &&_CAntidadeBalas > 0)
    {
        SetSlideLock(false);
        // bulletRenderer.enabled = true;
         //   casingRenderer.enabled = true;
        _isBulletChambered = true;
       
        
      
    }
    else if (_cartridgeStatus == CartridgeStatus.Chambered && _CAntidadeBalas <= 0)
    {
        _isSlideLocked = true;
        SetSlideLock(true);
         _cartridgeStatus = CartridgeStatus.None;
        
        // Debug.Log("No hay balas para cargar en la recámara.");


    }
}

     public void CargadorInsertado(bool isCargador)
        {
            
            _CargadorInsertado = isCargador;
            _CAntidadeBalas=GetBulletCount();
        }


    private bool CanFire()
    {
        return _cartridgeStatus == CartridgeStatus.Chambered && _isBulletChambered;
    }

     public void TransitRound()
        {
            if (_cartridgeStatus != CartridgeStatus.None)
                return;


            _cartridgeStatus = CartridgeStatus.ToChamber;
         
            //bulletRenderer.enabled = true;
            //casingRenderer.enabled = true;
        }

         private void SetSlideLock(bool isLocked)
        {
            _isSlideLocked = isLocked;
            ArticulationDrive drive = _slideBody.zDrive;
            drive.upperLimit = isLocked ? -0.04f : 0;
            _slideBody.zDrive = drive;
        }




    private void PlaySound(AudioClip[] clips)
    {
        _audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
