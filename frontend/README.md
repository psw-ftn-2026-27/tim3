# Explorer frontend

Angular application for the Explorer platform.

## Running

```bash
npm install
npm start
```

The app runs on `http://localhost:4200/`. Requests to `/api` are proxied to the
backend, which is expected on `http://localhost:5000`.

## Styling

A page uses the shared classes for its states (loading, empty state, empty, error,
success), buttons, badges, cards, form fields, tables and page actions. The guide in
[src/styles/README.md](src/styles/README.md) (in Serbian) says when to use each class,
with an example to copy. Pages in a pull request are reviewed against it.
