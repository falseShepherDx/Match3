using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x, y;
    private Item _item;
    public Item Item
    {
        get => _item;
        set
        {
            if (_item == value) return;
            _item = value;
            icon.sprite = Item.sprite;

        }
    }

    public Image icon;
    public Button button;
    public Tile Left => x > 0 ? Board.Instance.Tiles[x - 1, y] : null;
    public Tile Top => y > 0 ? Board.Instance.Tiles[x, y - 1] : null;
    public Tile Right => x < Board.Instance.width - 1 ? Board.Instance.Tiles[x + 1, y] : null;

    public Tile Bottom => y < Board.Instance.height - 1 ? Board.Instance.Tiles[x, y + 1] : null;

    public Tile[] Neighbours => new[]
    {
        Left,
        Top,
        Right,
        Bottom

    };

    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }

    public bool IsNeighbour(Tile other)
    {
        return Left == other || Right == other || Top == other || Bottom == other;
    }

    public List<Tile> GetConnectedTiles()
    {
        List<Tile> rowMatches = new List<Tile>();
        List<Tile> columnMatches = new List<Tile>();

       
        rowMatches = FindMatchesInDirection(this, true);

        columnMatches = FindMatchesInDirection(this, false);

        if (rowMatches.Count >= 3 && columnMatches.Count >= 3)
        {
            
            return rowMatches.Union(columnMatches).ToList();
        }
        if (rowMatches.Count >= 3)
            return rowMatches;
        if (columnMatches.Count >= 3)
            return columnMatches;

        return new List<Tile>();
    }

   
    private List<Tile> FindMatchesInDirection(Tile startTile, bool checkRow)
    {
        List<Tile> matchList = new List<Tile> { startTile };

        Tile negativeDir = checkRow ? startTile.Left : startTile.Top;
        Tile positiveDir = checkRow ? startTile.Right : startTile.Bottom;

    
        while (negativeDir != null && negativeDir.Item == startTile.Item)
        {
            matchList.Add(negativeDir);
            negativeDir = checkRow ? negativeDir.Left : negativeDir.Top;
        }

       
        while (positiveDir != null && positiveDir.Item == startTile.Item)
        {
            matchList.Add(positiveDir);
            positiveDir = checkRow ? positiveDir.Right : positiveDir.Bottom;
        }

        return matchList;
    }




}
