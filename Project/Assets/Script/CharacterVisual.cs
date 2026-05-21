using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    [SerializeField]
    private Transform modelParent;
    private Animator _animator;

    void Awake()
    {
        // ถ้าไม่ได้ assign ใน Inspector → ใช้ตัวเองแทน
        if (modelParent == null)
            modelParent = transform;
    }

    public void SetModel(string characterName)
    {
        foreach (Transform child in modelParent)
        {
            if (child.tag == "CharacterModel")
                Destroy(child.gameObject);
        }

        GameObject model = Resources.Load<GameObject>($"Characters/{characterName}");
        if (model != null)
        {
            var m = Instantiate(model, modelParent);
            _animator = m.GetComponent<Animator>();
        }
        else
            Debug.LogWarning($"Model not found: {characterName}");
    }

    public void SetWalking(bool isWalking)
    {
        _animator.SetBool("isWalking", isWalking);
    }

    public void PlayAttack()
    {
        _animator.SetTrigger("Attack");
    }

    public IEnumerator PlayAttackAndWait(Character target, float targetHp)
    {
        transform.LookAt(target.transform);
        PlayAttack();
        GameManager.OnAttacking?.Invoke();
        // ✅ รอให้เข้า state Attack ก่อน
        yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"));

        // ✅ แล้วค่อยรอให้จบ
        yield return new WaitUntil(() =>
            _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        target.TakeDamage(targetHp);
        GameManager.FinishAttacking?.Invoke();
    }
}
