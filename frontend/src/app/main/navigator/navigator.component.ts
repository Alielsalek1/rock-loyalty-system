import { Component } from '@angular/core';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-navigator',
  templateUrl: './navigator.component.html',
  styleUrl: './navigator.component.css',
})
export class NavigatorComponent {
  restaurantId: string;
  constructor(private authService: AuthService) {
    this.restaurantId = authService.restaurantId;
  }
}
