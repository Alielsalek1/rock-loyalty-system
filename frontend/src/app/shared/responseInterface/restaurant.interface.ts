export interface Restaurant {
    restaurantId: number;
    creditPointsBuyingRate: number;
    creditPointsSellingRate: number;
    loyaltyPointsBuyingRate: number;
    loyaltyPointsSellingRate: number;
    creditPointsLifeTime: number;
    loyaltyPointsLifeTime: number;
    voucherLifeTime: number;
}

export interface RestaurantUpdateRequest {
    creditPointsBuyingRate: number;
    creditPointsSellingRate: number;
    creditPointsLifeTime: number;
    voucherLifeTime: number;
}