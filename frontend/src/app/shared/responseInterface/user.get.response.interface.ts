export interface UserInterface {
  success: boolean;
  message: string;
  data: {
    accessToken: string;
    user: {
      id: number;
      email: string;
      phoneNumber: string;
      restaurantId: string;
      name: string;
    };
  };
}
