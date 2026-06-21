import {Component, Input} from '@angular/core';
import {PrincipalDto} from "../../../generated/client";
import {RouterLink} from "@angular/router";
import {SlicePipe, UpperCasePipe} from "@angular/common";

@Component({
  selector: 'app-principal-card',
  imports: [
    RouterLink,
    SlicePipe,
    UpperCasePipe
  ],
  templateUrl: './principal-card.html',
  styleUrl: './principal-card.css',
})
export class PrincipalCard {
  @Input({ required: true }) principal: PrincipalDto;
}
