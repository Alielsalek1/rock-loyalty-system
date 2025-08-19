import { Component } from '@angular/core';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-admin-navigator',
  templateUrl: './admin-navigator.component.html',
  styleUrl: './admin-navigator.component.css',
})
export class AdminNavigatorComponent {
  restaurantId: string;
  
  constructor(private authService: AuthService) {
    this.restaurantId = authService.restaurantId;
  }
  
  onLogOut() {
    this.authService.onAdminLogOut();
  }
}