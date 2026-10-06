namespace ExamGuide.Data;
// Один объект на окно входа. Источник истины — порядок фрагментов, а не флаг из интерфейса.
public sealed class Puzzle
{
    private readonly int[] tiles = [1, 2, 3, 4];
    public IReadOnlyList<int> Tiles => Array.AsReadOnly(tiles);
    public bool IsSolved => tiles.SequenceEqual(new[] { 1, 2, 3, 4 });
    public Puzzle() => Shuffle();
    public void Shuffle()
    {
        Random.Shared.Shuffle(tiles);
        if (IsSolved) Swap(0, 1);
    }
    public void Swap(int first, int second)
    {
        if (first is < 0 or > 3 || second is < 0 or > 3) throw new ArgumentOutOfRangeException();
        (tiles[first], tiles[second]) = (tiles[second], tiles[first]);
    }
}
