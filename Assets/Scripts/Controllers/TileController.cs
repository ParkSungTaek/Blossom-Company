using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using static Define;


public class TileController : MonoBehaviour
{
    /// <summary>
    /// 유일한 Controller
    /// </summary>
    static TileController _instance = new TileController();
    public static TileController Instance { get { init(); return _instance; } }


    #region 받아온 이미지들   
    // 여기 있으면 안될듯?
    Sprite[] _flowerSprites;
    Sprite[] _leafSprites;
    Sprite[] _witheredFlowersTileSprites;
    Sprite[] _bonusSprites;
    Sprite[] _cosmosFlowerSprites;

    public Sprite[] FlowerSprites { get { return _flowerSprites; } }
    public Sprite[] LeafSprites { get { return _leafSprites; } }
    public Sprite[] WitheredFlowersTileSprites { get { return _witheredFlowersTileSprites; } }
    public Sprite[] BonusSprites { get { return _bonusSprites; } }
    public Sprite[] CosmosFlowerSprites { get { return _cosmosFlowerSprites; } }


    #endregion 받아온 이미지들

    #region Tile 위치정보 관련
    const int _tileNum = 15;
    Vector3[] _tilePosition = new Vector3[_tileNum];
    public Vector3[] TilePosition { get { return _tilePosition; } }

    
    Vector3 _referenceTile = new Vector3(-12f,-3.5f,0);
    float _referenceDist = 3f;
    #endregion

    #region 지금Generated Tile들

    /// <summary>
    /// 희망은 LinkedList
    /// </summary>
    /// 
    //[SerializeField]
    List<Tile> _nowGeneratedTiles = new List<Tile>();
    public List<Tile> NowGeneratedTiles { get { return _nowGeneratedTiles; } }

    #endregion 지금Generated Tile들

    #region BackGround 통제
    public Action BackGroundMove { get; set; }
    public Vector3 DeltaMove { get; set; }
    #endregion

    #region TileSmoothMove

    public const float OVERTIME = 0.33f;
    public static bool IsMoving { get; set; }

    #endregion


    static Dictionary<TileType, Stack<Tile>> _poolingStack = new Dictionary<TileType, Stack<Tile>>();
    public static Dictionary<TileType, Stack<Tile>> PoolingStack { get { return _poolingStack; }}
    
    /// TileType => TileType.Flower == > Stack<Tile>중 어떤 객체
    /// 자동차 키 => BMW 16가 서울 6743 ==> 자동차 => BMW 16가 서울 6743 

    /// <summary>
    /// 존재함을 보장함
    /// </summary>
    /// 

    public static void init()
    {
        #region 3개의 꽃 타일 지정
        //3개 꽃 타일 이름 가져오는 부분 추후 구현해야함!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        string cosmos1 = "Cosmos";
        string cosmos2 = "cosmos2";
        string cosmos3 = "cosmos3";

        // 랜덤한 숫자 생성
        //Random random = new Random();
        //int randomNumber = random.Next(1, 4);
        int randomNumber = 1;

        // 랜덤한 숫자에 따라 문자열 선택
        string randomFlower = "";
        if (randomNumber == 1)
        {
            randomFlower = cosmos1;
        }
        else if (randomNumber == 2)
        {
            randomFlower = cosmos2;
        }
        else if (randomNumber == 3)
        {
            randomFlower = cosmos3;
        }
        #endregion

        if (_instance == null)
        {
            GameObject flowerControl = GameObject.Find("TileController");
            if (flowerControl == null)
            {
                flowerControl = new GameObject { name = "TileController" };
                flowerControl.AddComponent<TileController>();
            }
            _instance = flowerControl.GetComponent<TileController>();

            #region PoolingGenerate

            for(int i = 0; i< (int)TileType.MaxCount; i++)
            {
                _poolingStack[(TileType)i] = new Stack<Tile>();
            }
            
            #endregion PoolingGenerate

            #region GetTileSprites
            string[] flowersSpritesStr = Enum.GetNames(typeof(Define.FlowerTypes));
            _instance._flowerSprites = new Sprite[(int)Define.FlowerTypes.MaxCount];
            for(int i = 0 ; i< (int)Define.FlowerTypes.MaxCount; i++)
            {
                _instance._flowerSprites[i] = Resources.Load<Sprite>($"Sprites/Flowers/{flowersSpritesStr[i]}"); 
                if(_instance._flowerSprites[i] == null)
                {
                    Debug.Log("_instance._flowerSprites[(int)i] NULL");
                }
            }
            string[] leavesSpritesStr = Enum.GetNames(typeof(Define.LeafTypes));
            _instance._leafSprites = new Sprite[(int)Define.LeafTypes.MaxCount];
            for (int i = 0; i < (int)Define.LeafTypes.MaxCount; i++)
            {
                _instance._leafSprites[i] = Resources.Load<Sprite>($"Sprites/Leaves/{leavesSpritesStr[i]}");
                if (_instance._leafSprites[i] == null)
                {
                    Debug.Log("_instance.Leaves[(int)i] NULL");
                }

            }
            string[] witheredFlowersTileTypesStr = Enum.GetNames(typeof(Define.WitheredFlowersTileTypes));
            _instance._witheredFlowersTileSprites = new Sprite[(int)Define.WitheredFlowersTileTypes.MaxCount];
            for (int i = 0; i < (int)Define.WitheredFlowersTileTypes.MaxCount; i++)
            {
                _instance._witheredFlowersTileSprites[i] = Resources.Load<Sprite>($"Sprites/WitheredFlowersTileTypes/{witheredFlowersTileTypesStr[i]}");
                if (_instance._witheredFlowersTileSprites[i] == null)
                {
                    Debug.Log("_instance.WitheredFlowersTileTypes[(int)i] NULL");
                }

            }

            string[] BonusTileTypesStr = Enum.GetNames(typeof(Define.BonusTileTypes));
            _instance._bonusSprites = new Sprite[(int)Define.BonusTileTypes.MaxCount];
            for (int i = 0; i < (int)Define.BonusTileTypes.MaxCount; i++)
            {
                _instance._bonusSprites[i] = Resources.Load<Sprite>($"Sprites/BonusTiles/{BonusTileTypesStr[i]}");
                if (_instance._bonusSprites[i] == null)
                {
                    Debug.Log("_instance.BonusTileTypesStr[(int)i] NULL");
                }

            }

            string[] cosmosFlowersSpritesStr = Enum.GetNames(typeof(Define.CosmosFlower));
            _instance._cosmosFlowerSprites = new Sprite[(int)Define.CosmosFlower.MaxCount];
            for (int i = 0; i < (int)Define.CosmosFlower.MaxCount; i++)
            {
                _instance._cosmosFlowerSprites[i] = Resources.Load<Sprite>($"Sprites/Flowers/{randomFlower}/{cosmosFlowersSpritesStr[i]}");
                if (_instance._cosmosFlowerSprites[i] == null)
                {
                    Debug.Log("_instance._cosmosFlowerSprites[(int)i] NULL");
                }

            }
            #endregion GetTileSprites

            #region TileGenerateInit
            float X = _instance._referenceTile.x;
            Vector3 now = _instance._referenceTile;
            for (int i = 0; i < _tileNum; i++)
            {
                _instance._tilePosition[i] = now;
                now.x += _instance._referenceDist;
            }
            for (int i = 0; i < _tileNum; i++)
            {
                _instance.GeneratedTile(i);
            }

            #endregion

            #region TileSmoothMove
            _instance.DeltaMove = new Vector3(0.5f, 0, 0);
            IsMoving = false;
            #endregion
}
    }

    GameObject GeneratedTile(int tilePos = _tileNum-1)
    {
        TileType generateType = (TileType)UnityEngine.Random.RandomRange(0, (int)TileType.MaxCount);
        if (_poolingStack[generateType].Count != 0)
        {
            Tile generateTile = _poolingStack[generateType].Pop();

            GameObject gameObject = generateTile.gameObject;
            gameObject.SetActive(true);

            generateTile.Init();
            generateTile.TilePosititon = tilePos;
            _instance._nowGeneratedTiles.Add(generateTile);
            gameObject.transform.position = _instance._tilePosition[tilePos];
            return gameObject;
        }
        else
        {
            Tile generateTile;
            string generateTileStr = Enum.GetName(typeof(TileType), generateType);
            GameObject gameObject = Instantiate(Resources.Load<GameObject>($"Prefabs/{generateTileStr}/{generateTileStr}"), _instance._tilePosition[tilePos] ,Quaternion.identity);
            generateTile = gameObject.GetComponent<Tile>();
            generateTile.Init();
            generateTile.TilePosititon = tilePos;
            gameObject.transform.position = _instance._tilePosition[tilePos];
            _instance._nowGeneratedTiles.Add(generateTile);
            return gameObject;
        }


    }

    public Define.FlowerTypes SetFlowerType()
    {
        return GameManager.InGameDataManager.UseFlowerList[UnityEngine.Random.RandomRange(0, InGameDataManager.useFlowerNum)];
    }
    public Define.LeafTypes SetLeafType()
    {
        return (Define.LeafTypes)UnityEngine.Random.RandomRange(0, (int)Define.LeafTypes.MaxCount);
    }
    public Define.WitheredFlowersTileTypes SetWitheredFlowersType()
    {
        return (Define.WitheredFlowersTileTypes)UnityEngine.Random.RandomRange(0, (int)Define.WitheredFlowersTileTypes.MaxCount);
    }
    public Define.BonusTileTypes SetBonusType()
    {
        return (Define.BonusTileTypes)UnityEngine.Random.RandomRange(0, (int)Define.BonusTileTypes.MaxCount);
    }
    public Define.CosmosFlower SetCosmosFlowerType()
    {
        return (Define.CosmosFlower)UnityEngine.Random.RandomRange(0, (int)Define.CosmosFlower.MaxCount);
    }

    public void MoveTiles()
    {
        IsMoving = true;
        
        for(int i = 1; i < _tileNum; i++)
        {
            _instance._nowGeneratedTiles[i].MoveNext(i);
            
        }
        //맨 앞 지워줌
        _instance.DestoryTile(_instance._nowGeneratedTiles[0]);
        StartCoroutine(WaitTime());
        GeneratedTile();
        
        
    }
    
    
    public void DestoryTile(Tile tile)
    {
        tile.gameObject.SetActive(false);
        PoolingStack[tile.TileType].Push(tile);
        _instance._nowGeneratedTiles.RemoveAt(0);
    }
    public IEnumerator SmoothMove(Transform transform, Vector3 start, Vector3 end, float overTime = OVERTIME)
    {
        float startTime = Time.time;
        float endTime = startTime + overTime;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / overTime;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;

    }
    public IEnumerator WaitTime(float overTime = OVERTIME)
    {
        yield return new WaitForSeconds(overTime);
        IsMoving = false;
    }

}
