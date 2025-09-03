import { Component, OnDestroy, OnInit } from '@angular/core';
import { PointsService } from '../points/points.service';
import { finalize, Subscription } from 'rxjs';
import { ExchangePointsService } from './exchange-points.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-exchange-points',
  templateUrl: './exchange-points.component.html',
  styleUrl: './exchange-points.component.css',
})
export class ExchangePointsComponent implements OnInit, OnDestroy {
  minimumExchangeAmount: number = 10;

  points: number;
  from: number = 0;
  to: number = 0;
  valid: boolean = false;
  lifeTime: number = 0;

  loadingMessage: string = '';
  loading: boolean;
  exchangeRate: number = 2;
  errorMessage: string;

  private pointsServiceSub: Subscription;
  private exhangeServiceSub: Subscription;
  private lifeTimeServiceSub: Subscription;
  constructor(
    private pointsService: ExchangePointsService,
    private toastrService: ToastrService
  ) { }

  ngOnInit(): void {
    this.pointsServiceSub = this.pointsService.points.subscribe((points) => {
      this.points = points;
    });

    this.exhangeServiceSub = this.pointsService.exhangeRate.subscribe(
      (rate) => {
        this.exchangeRate = rate;
      }
    );

    this.lifeTimeServiceSub = this.pointsService.voucherLifeTimeInMinutes.subscribe((lifeTime) => {
      this.lifeTime = lifeTime;
    });

    let cnt = 0;
    cnt++;
    this.loading = true;
    this.loadingMessage = 'Fetching data';
    this.pointsService
      .getPoints()
      .pipe(
        finalize(() => {
          cnt--;
          if (cnt == 0) this.loading = false;
        })
      )
      .subscribe({
        next: () => { },
        error: (error) => {
          console.log(error);
          this.toastrService.error('An error occured while fetching user data');
        },
      });

    cnt++;
    this.loading = true;
    this.loadingMessage = 'Fetching data';
    this.pointsService
      .getExchangeRate()
      .pipe(
        finalize(() => {
          cnt--;
          if (cnt == 0) this.loading = false;
        })
      )
      .subscribe({
        next: () => { },
        error: (error) => {
          console.log(error);
          this.toastrService.error('An error occured while fetching user data');
        },
      });

    this.errorMessage = '';
    this.valid = false
  }

  ngOnDestroy(): void {
    this.pointsServiceSub.unsubscribe();
    this.exhangeServiceSub.unsubscribe();
  }

  fromTyping() {
    this.to = this.from * this.exchangeRate;
    this.checkValues();
  }

  toTyping() {
    this.from = Math.round(this.to / this.exchangeRate);
    this.checkValues();
  }

  private checkValues() {
    this.errorMessage = '';
    this.valid = false;

    // if both values are zero
    if (!this.from || this.from <= 0 || !this.to || this.to <= 0) {
      this.valid = false;
      return;
    }

    // Check if points is a float/double
    if (this.from && !Number.isInteger(Number(this.from))) {
      this.errorMessage = 'Points shouldn\'t be a float';
      return;
    }

    // Check if EGP is a float/double
    if (this.to && !Number.isInteger(Number(this.to))) {
      this.errorMessage = 'EGP amount shouldn\'t be a float';
      return;
    }

    // Check minimum exchange amount
    if (this.to && this.to < this.minimumExchangeAmount) {
      this.errorMessage = `Minimum exchange amount is ${this.minimumExchangeAmount} EGP`;
      return;
    }

    // Check insufficient points
    if (this.from > this.points) {
      this.errorMessage = 'Insufficient points';
      return;
    }

    this.valid = true;
  }

  onSubmit() {
    this.loading = true;
    this.loadingMessage = 'Creating voucher';
    this.pointsService.createVoucher(this.from).subscribe({
      next: () => {
        this.loading = false;
        this.toastrService.success('Voucher Created!');
      },
      error: (error) => {
        console.log(error);
        this.loading = false;
        this.toastrService.error('An error occured !');
      },
    });
  }
}
