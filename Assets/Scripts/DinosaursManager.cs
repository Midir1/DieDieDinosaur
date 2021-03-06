using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class DinosaursManager : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private float speed, idleWait;
    [SerializeField] private bool isVelociraptor, isTRex;

    [SerializeField] private AudioClip death;

    private AudioSource _audio;

    private bool _isDead;

    private Vector2 _groundSize;

    private bool _hasDestination, _stopDinosaur, _invincible;

    private Vector3 _startPosition, _currentDestination;

    private const float DestinationRadius = 0.1f;

    private float _timeElapsed, _waitTimer;

    private SpriteRenderer _spriteRenderer;

    private Animator _animator;

    private const string RunAnimation = "Run", DeathAnimation = "Death", HitAnimation = "Hit", IdleAnimation = "Idle";
    
    public int Damage
    {
        get => health;
        set
        {
            if (_isDead) return;
            
            health = value;

            TakeDamage();
        }
    }

    public int DamageMeteor
    {
        get => health;
        set
        {
            if (_invincible || _isDead) return;
            
            health = value;

            TakeDamage();
        }
    }

    [UsedImplicitly] 
    public void StartMoveAnimation()
    {
        _stopDinosaur = false;
        _animator.Play(RunAnimation);
    }

    [UsedImplicitly] public void DestroyGameObject() => Destroy(gameObject);
    
    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        
        _groundSize = new Vector2(8, 4);

        _startPosition = transform.position;
        _currentDestination = new Vector3(0, 0, transform.position.z);
        
        _animator.Play(RunAnimation);
    }

    private void Update()
    {
        if (_stopDinosaur) return;
        
        SetDestination();
        CheckDinosaurReachDestination();
        GoToDestination();
        FlipDinosaur();
    }

    private void SetDestination()
    {
        if (_hasDestination) return;
        
        var x = Random.Range(-_groundSize.x, _groundSize.x);
        var y = Random.Range(-_groundSize.y, _groundSize.y);

        _startPosition = transform.position;
        _currentDestination = new Vector3(x, y, transform.position.z);

        _timeElapsed = 0f;
        
        _hasDestination = true;
    }

    private void CheckDinosaurReachDestination()
    {
        if(!_hasDestination) return;

        if (Vector3.Distance(transform.position, _currentDestination) > DestinationRadius) return;
        
        StopDinosaurAnimation();
            
        _animator.Play(IdleAnimation);
        _waitTimer += Time.deltaTime;

        if (_waitTimer < idleWait) return;
        
        _waitTimer = 0f;

        StopDinosaurAnimation();

        _animator.Play(RunAnimation);

        _hasDestination = false;
    }

    private void GoToDestination()
    {
        transform.position = Vector3.Lerp(_startPosition, _currentDestination, _timeElapsed / Vector3.Distance(_startPosition, _currentDestination));
        _timeElapsed += speed * Time.deltaTime;
    }

    private void FlipDinosaur()
    {
        var distanceX = _currentDestination.x - transform.position.x;

        _spriteRenderer.flipX = distanceX < 0;
    }
    
    private void TakeDamage()
    {
        StopDinosaurMovement();
            
        if (health > 0)
        {
            _audio.Play();
                
            _animator.Play(HitAnimation);
        }
        else
        {
            _isDead = true;
                
            _audio.clip = death;
            _audio.Play();
                
            _animator.Play(DeathAnimation);
        }
    }
    
    private void StopDinosaurMovement()
    {
        _stopDinosaur = true;
        _timeElapsed = 0f;
        _waitTimer = 0f;
        _hasDestination = false;

        StopDinosaurAnimation();
    }

    private void StopDinosaurAnimation()
    {
        _animator.enabled = false;
        _animator.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag("Bush") && isVelociraptor) _invincible = true;
        else if (col.transform.CompareTag("Tree") && (isVelociraptor || isTRex)) _invincible = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.transform.CompareTag("Bush") && isVelociraptor) _invincible = false;
        else if (col.transform.CompareTag("Tree") && (isVelociraptor || isTRex)) _invincible = false;
    }
}