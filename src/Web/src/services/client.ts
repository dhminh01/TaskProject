import { ApolloClient, HttpLink, InMemoryCache } from "@apollo/client";

// Create Apollo Client
const client = new ApolloClient({
  link: new HttpLink({ uri: "http://localhost:5000/tasks" }),
  cache: new InMemoryCache(),
});

export default client;
