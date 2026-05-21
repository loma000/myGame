using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image _healthSprite;

    [SerializeField]
    private float _reduceSpeed = 2;
    private float _target = 1;
    private Camera _camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = Camera.main;
    }

    public void UpdateHealthBar(float maxHp, float currentHp)
    {
        _target = currentHp / maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(
            transform.position - _camera.transform.position
        );
        _healthSprite.fillAmount = Mathf.MoveTowards(
            _healthSprite.fillAmount,
            _target,
            _reduceSpeed
        );
    }
}
