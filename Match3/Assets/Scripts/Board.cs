using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using System;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;
    public Tile[,] Tiles { get; private set; }

    public int width => Tiles.GetLength(0);
    public int height => Tiles.GetLength(1);
    public RectTransform[] spawners;

    // private readonly List<Tile> _selection = new List<Tile>();
    public Tile SelectedTile { get; private set; }

    [SerializeField] private const float tweenDuration = 0.25f;

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        int height = rows.Length;
        int width = rows[0].tiles.Length;

        Tiles = new Tile[width, height];

        for (int y = 0; y < height; y++)
        {
            if (rows[y].tiles.Length != width)
            {
                Debug.LogError($"Row {y} has incorrect tile count!");
                continue;
            }

            for (int x = 0; x < width; x++)
            {
                Tile tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                
                tile.InitializeItem(ItemDataBase.Items[UnityEngine.Random.Range(0, ItemDataBase.Items.Length)]);

                Tiles[x, y] = tile;
            }
        }

        if (CanPop())
        {
            Pop();
        }
    }



    public async void Select(Tile tile)
    {

        if (SelectedTile == null)
        {
            SelectedTile = tile;
            return;
        }
        if (!SelectedTile.IsNeighbour(tile))
        {
            SelectedTile = null;
            return;
        }


        Debug.Log($"Swapping tiles at ({SelectedTile.x},{SelectedTile.y}) and ({tile.x},{tile.y})");

        await Swap(SelectedTile, tile);


        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(SelectedTile, tile);
        }

        SelectedTile = null; 
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;
        Sequence sequence = DOTween.Sequence();
        sequence
            .Join(icon1Transform.DOMove(icon2Transform.position, tweenDuration))
            .Join(icon2Transform.DOMove(icon1Transform.position, tweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1item = tile1.Item;

        tile1.Item = tile2.Item;
        tile2.Item = tile1item;

    }

    private bool CanPop()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                {
                    return true;
                }
            }

        }
        return false;
    }
    private async void Pop()
    {
        bool hasMatch;
        do
        {
            hasMatch = false;
            HashSet<Tile> tilesToPop = new HashSet<Tile>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var connectedTiles = Tiles[x, y].GetConnectedTiles();

                    if (connectedTiles.Count >= 3)
                    {
                        hasMatch = true;
                        tilesToPop.UnionWith(connectedTiles);
                    }
                }
            }

            if (hasMatch)
            {
                Sequence popSequence = DOTween.Sequence();

                foreach (var tile in tilesToPop)
                {
                    Sequence singleTileSequence = DOTween.Sequence();

                    singleTileSequence
                        .Append(tile.icon.transform.DOScale(1.3f, 0.1f).SetEase(Ease.OutQuad))
                        .Append(tile.icon.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack))
                        .OnComplete(() => tile.Item = null);

                    popSequence.Join(singleTileSequence);
                }

                await popSequence.Play().AsyncWaitForCompletion();

                await ApplyGravity();
                await SpawnNewTiles();
            }

        } while (hasMatch);
    }

    private async Task ApplyGravity()
    {
        bool moved;
        do
        {
            moved = false;
            Sequence gravitySequence = DOTween.Sequence();

            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y > 0; y--)
                {
                    if (Tiles[x, y].Item == null && Tiles[x, y - 1].Item != null)
                    {
                        // visual changes
                        Tiles[x, y].Item = Tiles[x, y - 1].Item;
                        Tiles[x, y - 1].Item = null;

                        var iconTransform = Tiles[x, y].icon.transform;
                        var startPos = Tiles[x, y - 1].transform.position;
                        var endPos = Tiles[x, y].transform.position;

                        iconTransform.position = startPos;

                        //smoother gravity fall
                        gravitySequence.Join(iconTransform.DOMove(endPos, 0.3f).SetEase(Ease.InQuad));

                        moved = true;
                    }
                }
            }

            if (moved)
                await gravitySequence.Play().AsyncWaitForCompletion();

        } while (moved);
    }
    private async Task SpawnNewTiles()
    {
        Sequence sequence = DOTween.Sequence();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (Tiles[x, y].Item == null)
                {
                    Tiles[x, y].SetItemAnimated(ItemDataBase.Items[UnityEngine.Random.Range(0, ItemDataBase.Items.Length)]);
                    Tiles[x, y].icon.transform.position = spawners[x].position;
                    Tiles[x, y].icon.transform.localScale = Vector3.one;

                    sequence.Join(Tiles[x, y].icon.transform.DOMove(Tiles[x, y].transform.position, 0.5f).SetEase(Ease.OutQuad));
                }
            }
        }

        await sequence.Play().AsyncWaitForCompletion();

       
    }

   

}