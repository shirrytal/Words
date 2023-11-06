using System.Text;

public class Word
{
    private readonly string word;

    public Word(string word)
    {
        this.word = word;
    }

    public Word(StringBuilder builder)
    {
        this.word = builder.ToString();
    }

    public string GetWord()
    {
        return word;
    }
}
