import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import { ApolloClient, HttpLink, InMemoryCache } from "@apollo/client";
import { ApolloProvider } from "@apollo/client/react";
import { GET_TASKS } from "./graphql/queries";

// Create Apollo Client
const client = new ApolloClient({
  link: new HttpLink({ uri: "http://localhost:5000/tasks" }),
  cache: new InMemoryCache(),
});

client
  .query({
    query: GET_TASKS,
  })
  .then((result) => console.log(result));

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <ApolloProvider client={client}>
      <App />
    </ApolloProvider>
  </React.StrictMode>
);
