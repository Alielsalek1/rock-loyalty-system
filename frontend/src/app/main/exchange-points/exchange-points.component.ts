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
  points: number;
  from: number = 0;
  to: number = 0;
  valid: boolean = false;
  lifeTime: number = 0;

  loadingMessage: string = '';
  loading: boolean;
  exchangeRate: number = 2;
  private pointsServiceSub: Subscription;
  private exhangeServiceSub: Subscription;
  private lifeTimeServiceSub: Subscription;
  constructor(
    private pointsService: ExchangePointsService,
    private toastrService: ToastrService
  ) {}

  ngOnInit(): void {
    this.pointsServiceSub = this.pointsService.points.subscribe((points) => {
      this.points = points;
    });

    this.exhangeServiceSub = this.pointsService.exhangeRate.subscribe(
      (rate) => {
        this.exchangeRate = rate;
      }
    );

    this.lifeTimeServiceSub =
      this.pointsService.voucherLifeTimeInMinutes.subscribe((lifeTime) => {
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
        next: () => {},
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
        next: () => {},
        error: (error) => {
          console.log(error);
          this.toastrService.error('An error occured while fetching user data');
        },
      });
  }

  ngOnDestroy(): void {
    this.pointsServiceSub.unsubscribe();
    this.exhangeServiceSub.unsubscribe();
  }

  private checkValid() {
    console.log('to', this.to);
    console.log('from', this.from);
    console.log('exchange', this.exchangeRate);

    if (this.to < 1 || this.from % 1 != 0 || this.from > this.points) {
      this.valid = false;
    } else {
      this.valid = true;
    }
  }

  fromTyping() {
    this.to = this.exchangeRate * this.from;
    this.checkValid();
  }

  toTyping() {
    this.from = this.to / this.exchangeRate;
    this.checkValid();
  }

  onSubmit() {
    this.pointsService.createVoucher(this.from).subscribe({
      next: () => {
        this.toastrService.success('Voucher Created!');
      },
      error: (error) => {
        console.log(error);
        this.toastrService.error('An error occured !');
      },
    });
  }
}
