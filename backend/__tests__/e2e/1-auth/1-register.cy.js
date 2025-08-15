/// <reference types="cypress" />
describe('Register', () => {
    it('successfull registration test', () => {
        const name = Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
        const email = name + '@example.com';
        const password = 'Password123';
        const phoneNumber = Math.floor(Math.random() * 10 ** 12).toString().padStart(12, '0');
        const restaurantId = 600;
        Cypress.env('email', email);
        Cypress.env('password', password);
        Cypress.env('phoneNumber', phoneNumber);
        Cypress.env('name', name);
        const body = {
            name,
            email,
            password,
            phoneNumber,
            restaurantId
        };
        cy.request({
            method: 'POST',
            url: `/api/users`,
            body,
            headers: {
                'Content-Type': 'application/json'
            },
            failOnStatusCode: false
        }).then(response => {
            cy.log('Api response: ' + JSON.stringify(response.body));
            expect(response.status).to.equal(200);
            Cypress.env('accessToken', JSON.stringify(response.body));
        })
    })
})