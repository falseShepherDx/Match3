using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x, y;
    private Item _item;

    public Item Item
    {
        get => _item;
        set => SetItem(value, false); 
    }

    public Image icon;
    public Button button;

    public Tile Left => x > 0 ? Board.Instance.Tiles[x - 1, y] : null;
    public Tile Top => y > 0 ? Board.Instance.Tiles[x, y - 1] : null;
    public Tile Right => x < Board.Instance.width - 1 ? Board.Instance.Tiles[x + 1, y] : null;
    public Tile Bottom => y < Board.Instance.height - 1 ? Board.Instance.Tiles[x, y + 1] : null;

    public Tile[] Neighbours => new[] { Left, Top, Right, Bottom };

    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }

    public bool IsNeighbour(Tile other)
    {
        return Neighbours.Contains(other);
    }

    private void SetItem(Item newItem, bool animated)
    {
        if (_item == newItem) return;
        _item = newItem;

        if (_item == null)
        {
            if (animated)
            {
                icon.transform.DOScale(Vector3.zero, 0.15f)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() => icon.enabled = false);
            }
            else
            {
                icon.enabled = false;
                icon.transform.localScale = Vector3.zero;
            }
        }
        else
        {
            icon.sprite = _item.sprite;
            icon.enabled = true;

            if (animated)
            {
                icon.transform.localScale = Vector3.zero;
                icon.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutCubic);
            }
            else
            {
                icon.transform.localScale = Vector3.one;
            }
        }
    }

    
    public void InitializeItem(Item newItem)
    {
        SetItem(newItem, false);
    }

   
    public void SetItemAnimated(Item newItem)
    {
        SetItem(newItem, true);
    }

    public List<Tile> GetConnectedTiles()
    {
        List<Tile> rowMatches = FindMatchesInDirection(this, true);
        List<Tile> columnMatches = FindMatchesInDirection(this, false);

        if (rowMatches.Count >= 3 && columnMatches.Count >= 3)
            return rowMatches.Union(columnMatches).ToList();

        if (rowMatches.Count >= 3) return rowMatches;
        if (columnMatches.Count >= 3) return columnMatches;

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
    public void PopAnimation()
    {
        // Cancel any existing scale animations to avoid conflicts
        icon.transform.DOKill();

        // Sequence: scale up slightly then scale down smoothly
        Sequence popSequence = DOTween.Sequence();
        popSequence.Append(icon.transform.DOScale(1.3f, 0.1f).SetEase(Ease.OutQuad));
        popSequence.Append(icon.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack));

        popSequence.OnComplete(() =>
        {
            icon.enabled = false;
            icon.transform.localScale = Vector3.one;
        });
    }
}
