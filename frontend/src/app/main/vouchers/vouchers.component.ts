import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { NgModel } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { VouchersService } from './vouchers.service';
import { Voucher } from './vouchers.interface';
import { finalize, Subscription } from 'rxjs';

@Component({
  selector: 'app-vouchers',
  templateUrl: './vouchers.component.html',
  styleUrl: './vouchers.component.css',
})
export class VouchersComponent implements OnInit, OnDestroy {
  @ViewChild('errorInput') errorInput: NgModel;
  vouchers: Voucher[] = [];

  loading: boolean = false;
  loadingMessage: string = '';

  totalItems = 9;
  itemsPerPage = 9;
  vouchersSub: Subscription;

  constructor(
    private toastrService: ToastrService,
    private vouchersService: VouchersService
  ) {}

  pageChanged(paginator: any) {
    this.loading = true;
    this.loadingMessage = 'Fetching vouchers';
    this.vouchersService
      .getVouchers(paginator.page, this.itemsPerPage)
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
    this.vouchersSub = this.vouchersService.vouchers.subscribe(
      (vouchers: Voucher[]) => {
        this.vouchers = vouchers;
      }
    );
    this.loading = true;
    this.loadingMessage = 'Fetching vouchers';
    this.vouchersService
      .getVouchers(1, this.itemsPerPage)
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
    this.vouchersSub.unsubscribe();
  }
  
  private resetPage() {
    this.vouchersService.getVouchers(1, this.itemsPerPage).subscribe({
      next: () => {},
      error: () => {
        this.toastrService.error(`Couldn't fetching vouchers`);
      },
    });
  }
}
