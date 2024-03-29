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

  public static attemptStatusToString(status: number): string {
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Syntax error';
      case 2: return 'Wrong answer';
      case 3: return 'Wrong result set format';
      case 4: return 'Time limit exceeded';
      case 5: return 'Accepted';
      default: return 'Unknown';
    }
  }

  
}
