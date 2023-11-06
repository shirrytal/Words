using System.Text;

public class CrossWordsTable
{

    public static int MAX_WORD_SIZE = 5;

	private readonly char[][] table;

	public CrossWordsTable(StringBuilder[] b) {
        table = new char[b.Length][]; 
        for (int i = 0; i < b.Length; i++)
        {
            table[i] = new char[MAX_WORD_SIZE];
            for (int j = 0; j < b[i].Length; j++)
            {
                table[i][j] = b[i][j];
            }
        }
    }

    public char[][] GetTable() {
        return table;
    }
    
}

