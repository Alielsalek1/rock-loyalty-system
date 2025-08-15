/// <reference types="cypress" />

describe('Login', () => {
    it('failed login test', () => {
        cy.request({
            method: 'POST',
            url: `/api/auth/login`,
            body: {
                email: 'test',
                password: 'test',
                restaurantId: 600
            },
            failOnStatusCode: false
        }).then(response => {
            expect(response.status).to.equal(401);
        })
    });
    it('successfull login test', () => {
        const email = Cypress.env('email');
        const password = Cypress.env('password');
        const restaurantId = 600;
        const body = {
            email,
            password,
            restaurantId
        };
        cy.request({
            method: 'POST',
            url: `/api/auth/login`,
            body,
            headers: {
                'Content-Type': 'application/json'
            },
            failOnStatusCode: false
        }).then(response => {
            cy.log('Api response: ' + JSON.stringify(response.body));
            cy.log(JSON.stringify(response.body));
            expect(response.status).to.equal(200);
            expect(response.body).to.not.be.null;
            expect(response.body).to.not.be.undefined;
            expect(response.body).to.not.be.empty;
            expect(response.headers['set-cookie']).to.not.be.empty;
            Cypress.env('accessToken', JSON.stringify(response.body));
        })
    });
})