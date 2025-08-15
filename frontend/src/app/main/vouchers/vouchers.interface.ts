export interface Voucher {
  shortCode: string;
  value: number;
  isUsed: boolean;
  DateOfCreation: Date;
}

export interface PaginationMetadata {
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
  dateOfCreation: Date;
}

export interface VouchersResponse {
  success: boolean;
  message: string;
  data: {
    vouchers: Voucher[];
  };
  metadata: PaginationMetadata;
}
