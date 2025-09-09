import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { TaskList } from "../../components/layout/TaskList";
import { BrowserRouter } from "react-router-dom";
import { ApolloClient, InMemoryCache } from "@apollo/client";
import { ApolloProvider } from "@apollo/client/react";
import { HttpLink } from "@apollo/client";

// Create a mock Apollo Client
const mockClient = new ApolloClient({
  link: new HttpLink({ uri: "http://localhost:5000/tasks" }),
  cache: new InMemoryCache(),
  defaultOptions: {
    watchQuery: { fetchPolicy: "no-cache" },
    query: { fetchPolicy: "no-cache" },
  },
});

// Mock Apollo hooks
vi.mock("@apollo/client/react", () => {
  const mockUseQuery = () => ({
    loading: false,
    error: null,
    data: {
      tasks: [
        {
          id: "1",
          title: "Test Task",
          description: "Test Description",
          dueDate: "2025-09-09",
          dateCreated: "2025-09-08",
        },
      ],
    },
    refetch: vi.fn(),
  });

  const mockUseMutation = () =>
    [vi.fn(), { loading: false, error: null }] as const;

  const mockApolloProvider = ({ children }: { children: React.ReactNode }) =>
    children;

  return {
    useQuery: mockUseQuery,
    useMutation: mockUseMutation,
    ApolloProvider: mockApolloProvider,
  };
});

const renderWithProviders = (component: React.ReactNode) => {
  return render(
    <BrowserRouter>
      <ApolloProvider client={mockClient}>{component}</ApolloProvider>
    </BrowserRouter>
  );
};

describe("TaskList", () => {
  it("renders the create task button", () => {
    renderWithProviders(<TaskList />);
    expect(screen.getByText("Create New Task")).toBeInTheDocument();
  });

  it("renders task data correctly", () => {
    renderWithProviders(<TaskList />);
    expect(screen.getByText("Test Task")).toBeInTheDocument();
    expect(screen.getByText("Test Description")).toBeInTheDocument();
    expect(screen.getByText("9/9/2025")).toBeInTheDocument();
  });

  it("renders action buttons for each task", () => {
    renderWithProviders(<TaskList />);
    expect(screen.getByTitle("Edit")).toBeInTheDocument();
    expect(screen.getByTitle("Delete")).toBeInTheDocument();
  });
});
