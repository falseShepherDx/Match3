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

    // private readonly List<Tile> _selection = new List<Tile>();
    public Tile SelectedTile { get; private set; }

    [SerializeField] private const float tweenDuration = 0.25f;

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        
            Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tile tile = rows[y].tiles[x];

                    tile.x = x;
                    tile.y = y;
                    tile.Item = ItemDataBase.Items[UnityEngine.Random.Range(0, ItemDataBase.Items.Length)];
                    Tiles[x, y] = tile;
                }
            }
        if(CanPop())
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

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = Tiles[x, y];
                var connectedTiles = tile.GetConnectedTiles();
                if (connectedTiles.Skip(1).Count() < 2) continue;

                Sequence deflateSequence = DOTween.Sequence();
                foreach (var connectedTile in connectedTiles)
                {
                    deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, tweenDuration));
                }
                await deflateSequence.Play().AsyncWaitForCompletion();

                var inflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = ItemDataBase.Items[UnityEngine.Random.Range(0, ItemDataBase.Items.Length)];

                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, tweenDuration));
                }
                await inflateSequence.Play().AsyncWaitForCompletion();
            }
        }

    }
}