using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    Rigidbody2D _myRgbd2D;
    float _jumpForce = 10f;
    bool _canJump;
    Tile _onTile;
    [SerializeField]
    Animator _animator;

    //public Sprite imageUnderThreshold;
    //public Sprite defaultImage;

    private SpriteRenderer spriteRenderer;
    bool isAnim = false;
    enum anims
    {
        Idle,
        Jump,
        Skip,
    }

    // Start is called before the first frame update
    void Start()
    {
        _canJump = true;
        _myRgbd2D = GetComponent<Rigidbody2D>();
        //animator = transform.GetChild(0).GetComponent<Animator>();
        _animator = Util.FindChild<Animator>(gameObject, "PlayerAnim");

    }


    public void Jump()
    {
        if (_canJump)
        {
            //����Ű ������ ��¼�� ��¼��
            _canJump = false;
            GameManager.InGameDataManager.NowState.JumpCnt++;
            _myRgbd2D.AddForce(new Vector3(0, 1f, 0) * _jumpForce, ForceMode2D.Impulse);
            StartCoroutine(AnimPlay(anims.Jump));
            _onTile.JumpOnMe();
 
        }
    }

    public void Skip()
    {
        StartCoroutine(AnimPlay(anims.Skip));


    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Flower")
        {
            _canJump = true;
            _onTile = collision.transform.GetComponent<Tile>();
        }
    }

    void Update()
    {
        
        if (transform.position.y < -9)
        {
            //transform.GetComponent<SpriteRenderer>().sprite = imageUnderThreshold;
        }
        else
        {

        }
    }
    
    IEnumerator AnimPlay(anims anim,float time = 0.33f)
    {
        if (!isAnim)
        {
            isAnim = true;
            string str = Enum.GetName(typeof(anims), anim);
            _animator.SetBool($"{str}Bool", true);
            yield return new WaitForSeconds(time);
            _animator.SetBool($"{str}Bool", false);
            isAnim = false;

        }



    }

}
