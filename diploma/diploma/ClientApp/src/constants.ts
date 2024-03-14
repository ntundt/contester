export class Constants {
  public static readonly EmptyGuid = '00000000-0000-0000-0000-000000000000';
  public static readonly OriginalityCheckThreshold = 5;
  public static readonly MaxInt = 2147483647;

  public static readonly monacoDefaultOptions = {
    theme:'vs-dark',
    language: 'markdown',
    minimap: {
      enabled: false,
    },
    wordWrap: 'on',
    unicodeHighlight: {
      ambiguousCharacters: false,
    },
  };
}
