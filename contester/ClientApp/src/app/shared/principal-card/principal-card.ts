import {Component, Input} from '@angular/core';
import {PrincipalDto} from "../../../generated/client";
import {RouterLink} from "@angular/router";
import {UpperCasePipe} from "@angular/common";
import {InitialsPipe} from "../../pipes/initials-pipe";
import {UuidToColorMapper} from "../uuid-to-color-mapper";
import {TranslatePipe} from "@ngx-translate/core";

@Component({
  selector: 'app-principal-card',
  imports: [
    RouterLink,
    InitialsPipe,
    UpperCasePipe,
    TranslatePipe
  ],
  templateUrl: './principal-card.html',
  styleUrl: './principal-card.css',
})
export class PrincipalCard {
  @Input({ required: true }) principal: PrincipalDto;

  protected readonly UuidToColorMapper = UuidToColorMapper;
}
