export interface Points {
  receiptId: number;
  restaurantId: number;
  customerId: number;
  transactionType: number;
  transactionDate: Date;
  transactionValue: number;
  isExpired: boolean;
  points: number;
}

export interface metaData {
  totalPages: number;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface PointsResponse {
  success: true;
  message: string;
  data: {
    transactionsResponse: Points[];
  };
  metadata: metaData;
}
