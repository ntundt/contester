import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'initials',
  standalone: true,
})
export class InitialsPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '';

    const words = value.trim().split(/[\s\d]+/);

    if (words.length >= 2) {
      const firstInitial = words[0].charAt(0);
      const secondInitial = words[1].charAt(0);
      return (firstInitial + secondInitial).toUpperCase();
    } else if (words.length === 1 && words[0].length > 0) {
      return words[0].substring(0, 2).toUpperCase();
    }

    return '';
  }
}
