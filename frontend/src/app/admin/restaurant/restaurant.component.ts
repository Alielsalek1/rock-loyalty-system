import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { RestaurantService } from '../restaurant.service';
import { Restaurant, RestaurantUpdateRequest } from '../../shared/responseInterface/restaurant.interface';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-restaurant',
  templateUrl: './restaurant.component.html',
  styleUrl: './restaurant.component.css'
})
export class RestaurantComponent implements OnInit {
  @ViewChild('restaurantForm') form: NgForm;
  
  loading: boolean = false;
  saving: boolean = false;
  restaurant: Restaurant | null = null;
  originalData: RestaurantUpdateRequest | null = null;

  formData: RestaurantUpdateRequest = {
    creditPointsBuyingRate: 0,
    creditPointsSellingRate: 0,
    creditPointsLifeTime: 0,
    voucherLifeTime: 0
  };

  constructor(
    private authService: RestaurantService,
    private toastrService: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadRestaurantData();
  }

  loadRestaurantData(): void {
    this.loading = true;
    const restaurantId = Number(this.authService.restaurantId);
    
    this.authService.getRestaurant(restaurantId)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (response) => {
          this.restaurant = response.data.restaurant;
          this.initializeFormData();
        },
        error: (error) => {
          this.toastrService.error(error.message);
        }
      });
  }

  private initializeFormData(): void {
    if (this.restaurant) {
      this.formData = {
        creditPointsBuyingRate: this.restaurant.creditPointsBuyingRate,
        creditPointsSellingRate: this.restaurant.creditPointsSellingRate,
        creditPointsLifeTime: this.restaurant.creditPointsLifeTime,
        voucherLifeTime: this.restaurant.voucherLifeTime
      };
      
      this.originalData = { ...this.formData };
    }
  }

  get hasChanges(): boolean {
    if (!this.originalData) return false;
    
    return (
      this.formData.creditPointsBuyingRate !== this.originalData.creditPointsBuyingRate ||
      this.formData.creditPointsSellingRate !== this.originalData.creditPointsSellingRate ||
      this.formData.creditPointsLifeTime !== this.originalData.creditPointsLifeTime ||
      this.formData.voucherLifeTime !== this.originalData.voucherLifeTime
    );
  }

  onSave(): void {
    if (!this.hasChanges || !this.restaurant) return;

    this.saving = true;
    const restaurantId = this.restaurant.restaurantId;

    this.authService.updateRestaurant(restaurantId, this.formData)
      .pipe(finalize(() => this.saving = false))
      .subscribe({
        next: () => {
          this.toastrService.success('Restaurant updated successfully');
          this.originalData = { ...this.formData };
        },
        error: (error) => {
          this.toastrService.error(error.message);
        }
      });
  }

  onReset(): void {
    if (this.originalData) {
      this.formData = { ...this.originalData };
    }
  }
}