import {Component, Input} from '@angular/core';
import {PrincipalDto} from "../../../generated/client";

@Component({
  selector: 'app-principal-card',
  imports: [],
  templateUrl: './principal-card.html',
  styleUrl: './principal-card.css',
})
export class PrincipalCard {
  @Input({ required: true }) principal: PrincipalDto;
}
