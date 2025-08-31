import { Component, ViewChild } from '@angular/core';
import { Voucher } from '../vouchers/vouchers.interface';
import { finalize, Subscription } from 'rxjs';
import { VouchersService } from '../vouchers/vouchers.service';
import { ToastrService } from 'ngx-toastr';
import { PointsService } from './points.service';
import { Points } from './points.interface';

@Component({
  selector: 'app-points',
  templateUrl: './points.component.html',
  styleUrl: './points.component.css',
})
export class PointsComponent {
  points: Points[] = [];

  loading: boolean = false;
  loadingMessage: string = '';

  totalItems = 10;
  itemsPerPage = 10;
  PointsSub: Subscription;

  constructor(
    private toastrService: ToastrService,
    private pointsService: PointsService
  ) { }

  pageChanged(paginator: any) {
    this.loading = true;
    this.loadingMessage = 'Fetching points';
    this.pointsService
      .getPointsList(paginator.page, this.itemsPerPage)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: (response) => {
          this.totalItems = response.metadata.totalCount;
        },
        error: () => {
          this.resetPage();
        },
      });
  }

  ngOnInit(): void {
    this.PointsSub = this.pointsService.pointsList.subscribe(
      (points: Points[]) => {
        this.points = points;

      }
    );
    this.loading = true;
    this.loadingMessage = 'Fetching points';
    this.pointsService
      .getPointsList(1, this.itemsPerPage)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: (response) => {
          this.totalItems = response.metadata.totalCount;
        },
        error: () => {
          this.resetPage();
        },
      });
  }

  ngOnDestroy(): void {
    this.PointsSub.unsubscribe();
  }
  
  private resetPage() {
    this.pointsService.getPointsList(1, this.itemsPerPage).subscribe({
      next: () => { },
      error: () => {
        this.toastrService.error(`Couldn't fetching points`);
      },
    });
  }
}
