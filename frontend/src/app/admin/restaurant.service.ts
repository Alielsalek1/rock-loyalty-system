import { Injectable } from '@angular/core';
import { Restaurant, RestaurantUpdateRequest } from '../shared/responseInterface/restaurant.interface';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../env';
import { User } from '../shared/modules/user.module';

@Injectable({
  providedIn: 'root'
})
export class RestaurantService {
  currentUser: User;
  restaurantId: string;

  user: BehaviorSubject<User> = new BehaviorSubject<User>(null);
  constructor(private http: HttpClient, private router: Router) {
    this.user.subscribe((user) => {
      this.currentUser = user;
    });
    const pathSegments = window.location.pathname.split('/');
    this.restaurantId = pathSegments[1] || '1234';
  }

  getRestaurant(restaurantId: number): Observable<any> {
    return this.http.get(`${environment.apiUrl}/api/admin/restaurants/${restaurantId}`)
      .pipe(
        catchError((errorResponse: HttpErrorResponse) => {
          return throwError(() => {
            return new Error(errorResponse.error.message || 'Failed to get restaurant data');
          });
        })
      );
  }
  
  updateRestaurant(restaurantId: number, data: RestaurantUpdateRequest): Observable<any> {
    return this.http.put(`${environment.apiUrl}/api/admin/restaurants/${restaurantId}`, data)
      .pipe(
        catchError((errorResponse: HttpErrorResponse) => {
          return throwError(() => {
            return new Error(errorResponse.error.message || 'Failed to update restaurant');
          });
        })
      );
  }
}
