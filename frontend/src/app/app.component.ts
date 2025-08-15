import { Component, OnInit } from '@angular/core';
import { AuthService } from './auth/auth.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private route: ActivatedRoute
  ) {}
  ngOnInit() {
    const restId = window.location.pathname.split('/')[1];

    if (restId) {
      this.authService.autoLogin();
    }
  }
}
