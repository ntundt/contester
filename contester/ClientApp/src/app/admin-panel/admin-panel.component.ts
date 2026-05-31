import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {RouterLink, RouterLinkActive, RouterOutlet} from "@angular/router";
import {faCheck, faCog, faDatabase, faListOl, faTasks, faUsers} from "@fortawesome/free-solid-svg-icons";

interface SidebarItem {
  id: number;
  icon: any;
  text: string;
  route: string;
  requiresPermission?: string;
}

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [FormsModule, TranslateModule, FaIconComponent, RouterLinkActive, RouterOutlet, RouterLink],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css'
})
export class AdminPanelComponent implements OnInit {
  public listItems: Array<SidebarItem> = [
    {id: 1, icon: faUsers, text: 'adminPanel.sidebar.users', route: 'users-control'},
    {id: 2, icon: faTasks, text: 'adminPanel.sidebar.connectionStrings', route: 'connection-strings'},
  ];

  ngOnInit() {

  }
}
