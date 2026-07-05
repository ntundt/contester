import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class UuidToColorMapper {

  public static uuidToColorDirect(uuid: string): string {
    const cleanUuid = uuid.replace(/-/g, '');
    const hexChunk = cleanUuid.substring(0, 4);

    const decimalValue = parseInt(hexChunk, 16);

    const hue = decimalValue % 360;

    const saturation = 50;
    const lightness = 50;

    return `hsl(${hue}, ${saturation}%, ${lightness}%)`;
  }

}
