using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WitheredFlowersTile : Tile
{
    public Define.WitheredFlowersTileTypes MyWitheredType { get; set; }

    public override void Init()
    {
        MyWitheredType = TileController.Instance.SetWitheredFlowersType();
        transform.GetComponent<SpriteRenderer>().sprite = TileController.Instance.WitheredFlowersTileSprites[(int)MyWitheredType];

    }
    public override void JumpOnMe()
    {
        GameManager.InGameDataManager.NowState.JumpCnt--;

        if (GameManager.InGameDataManager.NowState.LifeCnt != 1)
        {
            GameManager.UIManager.ShowSceneUI<GameUI>().LifeIcon.SetActive(false);  //��� ������ �Ҹ� : ������ ����
            GameManager.InGameDataManager.NowState.LifeCnt--;                       //��� ����
        }
        else
        {
            if (!GameManager.InGameDataManager.NowUnbeat)
            {
                TileController.Instance.DestoryTile(this);
                GameManager.SoundManager.Play(Define.SFX.Falling_02);//Falling_02ȿ����
            }
        }
        
        //Destroy(gameObject);
    }
    public override void SkipOnMe()
    {

    }
}
