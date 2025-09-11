import {Component, OnInit} from '@angular/core';
import {AuthenticationService} from "../../generated/client";
import {AuthenticationHelperService} from "../../authorization/authentication-helper.service";
import {ActivatedRoute, Router} from "@angular/router";

@Component({
  selector: 'app-email-code-sign-in',
  standalone: true,
  imports: [],
  templateUrl: './email-code-sign-in.component.html',
  styleUrl: './email-code-sign-in.component.css'
})
export class EmailCodeSignInComponent implements OnInit {
  public constructor(
    private authenticationService: AuthenticationService,
    private authenticationHelperService: AuthenticationHelperService,
    private router: Router,
    private route: ActivatedRoute,
  ) { }

  ngOnInit() {
    const email = this.route.snapshot.queryParamMap.get('email');
    const emailCode = this.route.snapshot.queryParamMap.get('emailCode');
    if (!email || !emailCode) return;

    this.authenticationService.apiAuthFinishPasswordlessSignInPost({ email, emailCode }).subscribe(res => {
      this.authenticationHelperService.setCredentials(res);
      this.router.navigate(['/'])
        .then(() => window.location.reload());
    });
  }
}
